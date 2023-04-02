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
using Newtonsoft.Json;

namespace SQLInjectionAnalyzer.Analyzers.InterproceduralSolution
{
    public class InterproceduralSolutionAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private string targetFileType = "*.sln";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole = false;
        private GlobalHelper globalHelper = new GlobalHelper();
        private DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        private TableOfRules tableOfRules = new TableOfRules();
        private InterproceduralHelper interproceduralHelper = new InterproceduralHelper();
        private List<string> excludeSubpaths = new List<string>();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.excludeSubpaths = excludeSubpaths;
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;
            Diagnostics diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.InterproceduralSolution);

            int numberOfSolutionFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
            int numberOfScannedSolutionFilesSoFar = 0;
            diagnostics.NumberOfSolutions = numberOfSolutionFilesUnderThisRepository;

            foreach (string solutionFilePath in Directory.EnumerateFiles(directoryPath, targetFileType, SearchOption.AllDirectories))
            {
                SolutionScanResult solutionScanResult = diagnosticsInitializer.InitializeSolutionScanResult(solutionFilePath);

                // skip all blacklisted .sln files
                if (excludeSubpaths.Any(x => solutionFilePath.Contains(x)))
                {
                    diagnostics.PathsOfSkippedSolutions.Add(solutionFilePath);
                }
                else
                {
                    Console.WriteLine("currently scanned .sln: " + solutionFilePath);
                    Console.WriteLine(numberOfScannedSolutionFilesSoFar + " / " + numberOfSolutionFilesUnderThisRepository + " .sln files scanned");
                    ScanSolution(solutionFilePath, solutionScanResult).Wait();
                    solutionScanResult.CSProjectScanResults.Add(csprojScanResult);
                }

                solutionScanResult.SolutionScanResultEndTime = DateTime.Now;

                diagnostics.SolutionScanResults.Add(solutionScanResult);
                numberOfScannedSolutionFilesSoFar++;
            }

            Console.WriteLine(numberOfScannedSolutionFilesSoFar + " / " + numberOfSolutionFilesUnderThisRepository + " .sln files scanned");

            diagnostics.DiagnosticsEndTime = DateTime.Now;

            return diagnostics;
        }

        private async Task ScanSolution(string solutionPath, SolutionScanResult solutionScanResult)
        {
            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                Solution solution = await workspace.OpenSolutionAsync(solutionPath);
                foreach (Project project in solution.Projects)
                {
                    solutionScanResult.NumberOfCSProjFiles++;
                    if (excludeSubpaths.Any(x => project.FilePath.Contains(x)))
                    {
                        solutionScanResult.PathsOfSkippedCSProjects.Add(project.FilePath);
                    }
                    else
                    {
                        Console.WriteLine("    + project: " + project.FilePath);
                        ScanCSProj(project, solution).Wait();
                    }
                }
            }
        }

        private async Task ScanCSProj(Project project, Solution solution)
        {
            csprojScanResult = diagnosticsInitializer.InitialiseCSProjectScanResult(project.FilePath);

            if (project.FilePath.EndsWith(".vbproj"))
            {
                csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
                return;
            }

            Compilation compilation = await project.GetCompilationAsync();

            foreach (CSharpSyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree, solution, compilation);
                csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
            }

            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree, Solution solution, Compilation compilation)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules, true)) continue;

                MethodScanResult methodScanResult = InterproceduralSolutionScanMethod(methodSyntax, solution, compilation);
                
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

        private MethodScanResult InterproceduralSolutionScanMethod(MethodDeclarationSyntax methodSyntax, Solution solution, Compilation compilation)
        {
            MethodScanResult methodScanResult = ConductScanOfTheInitialMethod(methodSyntax);
            SemanticModel semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree, ignoreAccessibility: false);
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);

            var random = new Random();
            InterproceduralTree tree = new InterproceduralTree
            {
                Id = random.Next(),
                MethodName = methodSymbol.ToString(),
                Callers = new List<InterproceduralTree>()
            };
            
            List<LevelBlock> currentLevelBlocks = new List<LevelBlock>() { new LevelBlock() { MethodSymbol = methodSymbol, TaintedMethodParameters = methodScanResult.TaintedMethodParameters, InterproceduralCallersTreeNodeId = tree.Id} };
            List<LevelBlock> nextLevelBlocks = new List<LevelBlock>();

            // n-level BFS solution interprocedural analysis
            for (int currentLevel = 2; currentLevel < taintPropagationRules.Level + 1; currentLevel++)
            {
                List<InvocationAndParentsTaintedParameters> allMethodInvocations = interproceduralHelper.FindAllCallersOfCurrentBlockInSolutionAsync(currentLevelBlocks, methodScanResult, solution, taintPropagationRules);

                foreach (InvocationAndParentsTaintedParameters invocation in allMethodInvocations)
                {
                    semanticModel = invocation.Compilation.GetSemanticModel(invocation.InvocationExpression.SyntaxTree, ignoreAccessibility: false);

                    MethodDeclarationSyntax parent = interproceduralHelper.FindMethodParent(invocation.InvocationExpression.Parent);

                    if (parent == null) continue;

                    int id = random.Next();
                    
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