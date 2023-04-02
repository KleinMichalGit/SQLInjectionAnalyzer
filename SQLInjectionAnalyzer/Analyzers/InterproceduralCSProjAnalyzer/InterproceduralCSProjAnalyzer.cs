﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.Solution;
using Model.SyntaxTree;
using SQLInjectionAnalyzer.Analyzers;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>InterproceduralCSProjAnalyzer</c> class.
    /// <para>
    /// Compiles all C# project (*.csproj) files, performs n-level
    /// interprocedural analysis (where number n is defined in config.json file)
    /// for each project separately, able to decide trivial conditional
    /// statements.
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer"/>
    public class InterproceduralCSProjAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private string targetFileType = "*.csproj";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole = false;
        private GlobalHelper globalHelper = new GlobalHelper();
        private DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        private TableOfRules tableOfRules = new TableOfRules();
        private InterproceduralHelper interproceduralHelper = new InterproceduralHelper();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;
            Diagnostics diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.InterproceduralCSProj);

            SolutionScanResult solutionScanResult = diagnosticsInitializer.InitializeSolutionScanResult(directoryPath);

            int numberOfCSProjFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
            int numberOfScannedCSProjFilesSoFar = 0;

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, targetFileType, SearchOption.AllDirectories))
            {
                solutionScanResult.NumberOfCSProjFiles++;

                // skip all blacklisted .csproj files
                if (excludeSubpaths.Any(x => filePath.Contains(x)))
                {
                    solutionScanResult.PathsOfSkippedCSProjects.Add(filePath);
                }
                else
                {
                    Console.WriteLine("currently scanned .csproj: " + filePath);
                    Console.WriteLine(numberOfScannedCSProjFilesSoFar + " / " + numberOfCSProjFilesUnderThisRepository + " .csproj files scanned");
                    ScanCSProj(filePath).Wait();
                    solutionScanResult.CSProjectScanResults.Add(csprojScanResult);
                }

                numberOfScannedCSProjFilesSoFar++;
            }

            Console.WriteLine(numberOfScannedCSProjFilesSoFar + " / " + numberOfCSProjFilesUnderThisRepository + " .csproj files scanned");

            diagnostics.SolutionScanResults.Add(solutionScanResult);

            diagnostics.DiagnosticsEndTime = DateTime.Now;

            return diagnostics;
        }

        private async Task ScanCSProj(string csprojPath)
        {
            csprojScanResult = diagnosticsInitializer.InitialiseCSProjectScanResult(csprojPath);

            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                Project project = await workspace.OpenProjectAsync(csprojPath);

                Compilation compilation = await project.GetCompilationAsync();

                foreach (CSharpSyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);

                    SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree, compilation);
                    csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                }
            }
            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree, Compilation compilation)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules)) continue;

                MethodScanResult methodScanResult = InterproceduralScanMethod(methodSyntax, compilation);

                // these values are not set for method scans without hits, because it resulted into OutOfMemoryException when analysing monorepository
                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = methodSyntax.Identifier.ToString() + methodSyntax.ParameterList.ToString();
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    FileLinePositionSpan lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    if (writeOnConsole)
                    {
                        globalHelper.WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence, methodScanResult);
                    }
                }

                if (methodScanResult.Hits == 0) // if all tainted variables are cleaned, we do not need to remember anything
                {
                    methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();
                }

                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }

        private MethodScanResult InterproceduralScanMethod(MethodDeclarationSyntax methodSyntax, Compilation compilation)
        {
            MethodScanResult methodScanResult = ConductScanOfTheInitialMethod(methodSyntax);
            SemanticModel semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree, ignoreAccessibility: false);
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);

            var random = new Random();
            InterproceduralTree tree = new InterproceduralTree
            {
                Id = random.Next(Int32.MaxValue),
                MethodName = methodSymbol.ToString(),
                Callers = new List<InterproceduralTree>()
            };
            
            List<LevelBlock> currentLevelBlocks = new List<LevelBlock>() { new LevelBlock() { MethodSymbol = methodSymbol, TaintedMethodParameters = methodScanResult.TaintedMethodParameters, InterproceduralCallersTreeNodeId = tree.Id} };
            List<LevelBlock> nextLevelBlocks = new List<LevelBlock>();
            
            // n-level BFS interprocedural analysis
            for (int currentLevel = 2; currentLevel < taintPropagationRules.Level + 1; currentLevel++)
            { 
                foreach (SyntaxTree currentSyntaxTree in compilation.SyntaxTrees)
                {
                    globalHelper.SolveSourceAreas(currentSyntaxTree, methodScanResult, taintPropagationRules); // source areas labels for more informative result
                    semanticModel = compilation.GetSemanticModel(currentSyntaxTree, ignoreAccessibility: false);

                    List<InvocationAndParentsTaintedParameters> allMethodInvocations = interproceduralHelper.FindAllCallersOfCurrentBlock(currentSyntaxTree, currentLevelBlocks, semanticModel, methodScanResult);

                    if (allMethodInvocations == null) return diagnosticsInitializer.InitialiseMethodScanResult();

                    foreach (InvocationAndParentsTaintedParameters invocation in allMethodInvocations)
                    {
                        MethodDeclarationSyntax parent = interproceduralHelper.FindMethodParent(invocation.InvocationExpression.Parent);

                        if (parent == null) continue;

                        int id = random.Next(Int32.MaxValue);
                        interproceduralHelper.AddCaller(
                            invocation.InterproceduralCallersTreeCalleeNodeId,
                            tree, 
                        new InterproceduralTree
                            {
                                Id = id,
                                MethodName = semanticModel.GetDeclaredSymbol(parent).ToString(),
                                Callers = new List<InterproceduralTree>()
                            }
                        );

                        methodScanResult.AppendEvidence("INTERPROCEDURAL LEVEL: " + currentLevel + " " + semanticModel.GetDeclaredSymbol(parent).ToString());

                        Tainted tainted = new Tainted()
                        {
                            TaintedMethodParameters = new int[parent.ParameterList.Parameters.Count()],
                            TaintedInvocationArguments = new int[invocation.InvocationExpression.ArgumentList.Arguments.Count()]
                        };

                        EvaluateRule(parent, invocation.InvocationExpression, methodScanResult, tainted, invocation.TaintedMethodParameters);

                        if (interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(invocation.TaintedMethodParameters, tainted.TaintedInvocationArguments))
                        {
                            methodScanResult.AppendEvidence("ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.");
                        }
                        else
                        {
                            nextLevelBlocks.Add(new LevelBlock() { TaintedMethodParameters = tainted.TaintedMethodParameters, MethodSymbol = semanticModel.GetDeclaredSymbol(parent), InterproceduralCallersTreeNodeId = id});
                        }
                    }
                }
               
                // on current level we have at least one method with tainted parameters, but this method has 0 callers. Therefore its parameters
                // will never be cleaned.
                if (interproceduralHelper.CurrentLevelContainsTaintedBlocksWithoutCallers(currentLevelBlocks))
                {
                    methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, THERE IS AT LEAST ONE METHOD WITH TAINTED PARAMETERS WITHOUT ANY CALLERS. THEREFORE, ITS PARAMETERS ARE UNCLEANABLE.");
                    methodScanResult.InterproceduralCallersTree = tree;
                    methodScanResult.MethodScanResultEndTime = DateTime.Now;
                    return methodScanResult;
                }

                currentLevelBlocks = nextLevelBlocks;
                nextLevelBlocks = new List<LevelBlock>();

                //all tainted parameters are cleaned
                if (currentLevelBlocks.Count() == 0)
                {
                    methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, ALL TAINTED VARIABLES WERE CLEANED.");
                    methodScanResult.InterproceduralCallersTree = tree;
                    methodScanResult.MethodScanResultEndTime = DateTime.Now;
                    return methodScanResult;
                }
            }

            methodScanResult.InterproceduralCallersTree = tree;
            
            methodScanResult.MethodScanResultEndTime = DateTime.Now;
            
            return methodScanResult;
        }

        private MethodScanResult ConductScanOfTheInitialMethod(MethodDeclarationSyntax methodSyntax)
        {
            MethodScanResult methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();
            methodScanResult.AppendEvidence("INTERPROCEDURAL LEVEL: 1 ");

            IEnumerable<InvocationExpressionSyntax> invocations = globalHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods);
            methodScanResult.Sinks = (short)invocations.Count();

            Tainted taintedMethod = new Tainted()
            {
                TaintedMethodParameters = new int[methodSyntax.ParameterList.Parameters.Count()],
            };

            // follows data flow inside method for each sink invocation from sink invocation to source
            foreach (var invocation in invocations)
            {
                Tainted taintedInvocation = new Tainted()
                {
                    TaintedMethodParameters = new int[methodSyntax.ParameterList.Parameters.Count()],
                    TaintedInvocationArguments = new int[invocation.ArgumentList.Arguments.Count()]
                };

                EvaluateRule(methodSyntax, invocation, methodScanResult, taintedInvocation);

                for (int i = 0; i < taintedMethod.TaintedMethodParameters.Length; i++)
                {
                    taintedMethod.TaintedMethodParameters[i] += taintedInvocation.TaintedMethodParameters[i];
                }
            }

            methodScanResult.TaintedMethodParameters = taintedMethod.TaintedMethodParameters;
            return methodScanResult;
        }

        private void EvaluateRule(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, Tainted tainted, int[] taintedMethodParameters = null, List<SyntaxNode> visitedNodes = null, int level = 0)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<SyntaxNode>();
            }
            else if (visitedNodes.Contains(currentNode))
            {
                result.AppendEvidence(new string(' ', level * 2) + "HALT (Node already visited)");
                return;
            }

            visitedNodes.Add(currentNode);
            result.AppendEvidence(new string(' ', level * 2) + currentNode.ToString());
            level += 1;

            SyntaxNode[] nextLevelNodes = null;

            if (currentNode is InvocationExpressionSyntax)
                nextLevelNodes = tableOfRules.SolveInvocationExpression((InvocationExpressionSyntax)currentNode, result, level, taintPropagationRules);
            else if (currentNode is ObjectCreationExpressionSyntax)
                nextLevelNodes = tableOfRules.SolveObjectCreationExpression((ObjectCreationExpressionSyntax)currentNode);
            else if (currentNode is AssignmentExpressionSyntax)
                nextLevelNodes = tableOfRules.SolveAssignmentExpression((AssignmentExpressionSyntax)currentNode);
            else if (currentNode is VariableDeclaratorSyntax)
                nextLevelNodes = tableOfRules.SolveVariableDeclarator((VariableDeclaratorSyntax)currentNode);
            else if (currentNode is ArgumentSyntax)
                nextLevelNodes = tableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is IdentifierNameSyntax)
                nextLevelNodes = tableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is ConditionalExpressionSyntax)
                nextLevelNodes = tableOfRules.SolveConditionalExpression((ConditionalExpressionSyntax)currentNode, result, level).Result;
            else if (currentNode is LiteralExpressionSyntax)
                tableOfRules.SolveLiteralExpression(result, level);
            else
                tableOfRules.SolveUnrecognizedSyntaxNode(result, currentNode, level);

            if (nextLevelNodes != null)
            {
                for (int i = 0; i < nextLevelNodes.Length; i++)
                {
                    // investigate only the origin of arguments which were previously tainted as method parameters
                    if (level == 1 && (taintedMethodParameters != null && taintedMethodParameters[i] == 0)) continue;

                    int numberOfTaintedMethodParametersBefore = tainted.TaintedMethodParameters.Count(num => num != 0);

                    EvaluateRule(rootNode, nextLevelNodes[i], result, tainted, null, visitedNodes, level);

                    // for the first invocation only
                    // for the parent's tainted method parameters only
                    // if following the data flow increased the number of tainted method parameters, it means that current argument of this invocationNode should be tainted
                    if (level == 1 && (taintedMethodParameters == null || taintedMethodParameters[i] > 0) && numberOfTaintedMethodParametersBefore < tainted.TaintedMethodParameters.Count(num => num != 0)) tainted.TaintedInvocationArguments[i] += 1;
                }
            }
        }
    }
}