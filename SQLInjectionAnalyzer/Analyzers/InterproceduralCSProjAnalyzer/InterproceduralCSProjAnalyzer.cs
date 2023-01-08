using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExceptionService.ExceptionType;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;
using Model;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>InterproceduralAnalyzer</c> class.
    /// 
    /// <para>
    /// Compiles *.csproj files, performs n-level interprocedural analysis,
    /// every block of code is considered as reachable
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer" />
    public class InterproceduralCSProjAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private string targetFileType = "*.csproj";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole = false;
        private GlobalHelper globalHelper = new GlobalHelper();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;
            Diagnostics diagnostics = globalHelper.InitialiseDiagnostics(ScopeOfAnalysis.InterproceduralCSProj);

            int numberOfCSProjFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
            int numberOfScannedCSProjFilesSoFar = 0;

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, targetFileType, SearchOption.AllDirectories))
            {
                diagnostics.NumberOfCSProjFiles++;

                // skip all blacklisted .csproj files
                if (excludeSubpaths.Any(x => filePath.Contains(x)))
                {
                    diagnostics.PathsOfSkippedCSProjects.Add(filePath);
                }
                else
                {
                    Console.WriteLine("currently scanned .csproj: " + filePath);
                    Console.WriteLine(numberOfScannedCSProjFilesSoFar + " / " + numberOfCSProjFilesUnderThisRepository + " .csproj files scanned");
                    ScanCSProj(filePath).Wait();
                    diagnostics.CSProjectScanResults.Add(csprojScanResult);
                }

                numberOfScannedCSProjFilesSoFar++;
            }

            Console.WriteLine(numberOfScannedCSProjFilesSoFar + " / " + numberOfCSProjFilesUnderThisRepository + " .csproj files scanned");

            diagnostics.DiagnosticsEndTime = DateTime.Now;

            return diagnostics;
        }

        private async Task ScanCSProj(string csprojPath)
        {
            csprojScanResult = globalHelper.InitialiseScanResult(csprojPath);

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
            SyntaxTreeScanResult syntaxTreeScanResult = new SyntaxTreeScanResult();
            syntaxTreeScanResult.SyntaxTreeScanResultStartTime = DateTime.Now;
            syntaxTreeScanResult.Path = syntaxTree.FilePath;

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules)) continue;

                MethodScanResult methodScanResult = ScanMethod(methodSyntax);

                // these values are not set for method scans without hits, because it resulted into OutOfMemoryException when analysing orion monorepository
                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = methodSyntax.Identifier.ToString() + methodSyntax.ParameterList.ToString();
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    FileLinePositionSpan lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    methodScanResult.AppendCaller("level | method");
                    SolveInterproceduralAnalysis(methodSyntax, compilation, syntaxTree, methodScanResult);

                    if (methodScanResult.Hits == 0) // if all tainted variables are cleaned, we do not need to remember anything
                    {
                        methodScanResult = globalHelper.InitialiseMethodScanResult();
                    }

                    if (methodScanResult.Hits > 0 && writeOnConsole)
                    {
                        globalHelper.WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence, methodScanResult);
                    }
                }

                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }

        private void SolveInterproceduralAnalysis(MethodDeclarationSyntax methodSyntax, Compilation compilation, SyntaxTree syntaxTree, MethodScanResult methodScanResult)
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree, ignoreAccessibility: false);
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);

            methodScanResult.AppendCaller("1 " + new string(' ', 2) + methodSymbol.ToString());
            List<LevelBlock> currentLevelBlocks = new List<LevelBlock>() { new LevelBlock() { MethodSymbol = methodSymbol, TaintedMethodParameters = methodScanResult.TaintedMethodParameters } };
            List<LevelBlock> nextLevelBlocks = new List<LevelBlock>();

            // n-level BFS interprocedural analysis
            for (int currentLevel = 2; currentLevel < taintPropagationRules.Level + 1; currentLevel++)
            {
                foreach (SyntaxTree currentSyntaxTree in compilation.SyntaxTrees)
                {
                    SolveSourceAreas(currentSyntaxTree, methodScanResult); // source areas labels for more informative result

                    semanticModel = compilation.GetSemanticModel(currentSyntaxTree, ignoreAccessibility: false);

                    List<InvocationAndParentsTaintedParameters> allMethodInvocations = FindAllCallersOfCurrentBlock(currentSyntaxTree, currentLevelBlocks, semanticModel, methodScanResult);

                    if (allMethodInvocations == null)
                    {
                        return;
                    }

                    foreach (InvocationAndParentsTaintedParameters invocation in allMethodInvocations)
                    {
                        MethodDeclarationSyntax parent = FindMethodParent(invocation.InvocationExpression.Parent);

                        if (parent != null)
                        {
                            methodScanResult.AppendCaller(currentLevel + " " + new string(' ', currentLevel * 2) + semanticModel.GetDeclaredSymbol(parent).ToString());
                            methodScanResult.BodiesOfCallers.Add(parent.ToString());
                            methodScanResult.AppendEvidence("INTERPROCEDURAL LEVEL: " + currentLevel + " " + semanticModel.GetDeclaredSymbol(parent).ToString());

                            Tainted tainted = new Tainted()
                            {
                                TaintedMethodParameters = new int[parent.ParameterList.Parameters.Count()],
                                TaintedInvocationArguments = new int[invocation.InvocationExpression.ArgumentList.Arguments.Count()]
                            };

                            FollowDataFlow(parent, invocation.InvocationExpression, methodScanResult, tainted, invocation.TaintedMethodParameters);

                            if (AllTaintVariablesAreCleanedInThisBranch(invocation.TaintedMethodParameters, tainted.TaintedInvocationArguments))
                            {
                                methodScanResult.AppendEvidence("ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.");
                            }
                            else
                            {
                                nextLevelBlocks.Add(new LevelBlock() { TaintedMethodParameters = tainted.TaintedMethodParameters, MethodSymbol = semanticModel.GetDeclaredSymbol(parent) });
                            }

                        }
                    }
                }


                // on current level we have at least one method with tainted parameters, but this method has 0 callers. Therefore its parameters
                // will be never cleaned.
                if (CurrentLevelContainsTaintedBlocksWithoutCallers(currentLevelBlocks))
                {
                    methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, THERE IS AT LEAST ONE METHOD WITH TAINTED PARAMETERS WHICH DOES NOT HAVE ANY CALLERS. THEREFORE ITS PARAMETERS ARE UNCLEANABLE.");
                    return;
                }

                currentLevelBlocks = nextLevelBlocks;
                nextLevelBlocks = new List<LevelBlock>();

                //all tainted parameters are cleaned
                if (currentLevelBlocks.Count() == 0)
                {
                    methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, ALL TAINTED VARIABLES WERE CLEANED. THEREFORE, THIS MESSAGE SHOULD NOT BE INCLUDED AMONG RESULTS.");
                    methodScanResult.Hits = 0;
                    return;
                }
            }
        }

        private bool CurrentLevelContainsTaintedBlocksWithoutCallers(List<LevelBlock> currentLevelBlocks)
        {
            foreach (LevelBlock levelBlock in currentLevelBlocks)
                if (levelBlock.TaintedMethodParameters.Sum() > 0 && levelBlock.NumberOfCallers == 0)
                    return true;

            return false;
        }

        private bool AllTaintVariablesAreCleanedInThisBranch(int[] parentMethodTainted, int[] invocationTainted)
        {
            if (parentMethodTainted.Length != invocationTainted.Length)
            {
                throw new AnalysisException("number of tainted method parameters and invocation arguments is incorrect!");
            }

            for (int i = 0; i < parentMethodTainted.Length; i++)
            {
                if (parentMethodTainted[i] > 0 && invocationTainted[i] > 0)
                    return false;
            }
            return true;
        }

        private List<InvocationAndParentsTaintedParameters> FindAllCallersOfCurrentBlock(SyntaxTree currentSyntaxTree, List<LevelBlock> currentLevelBlocks, SemanticModel semanticModel, MethodScanResult methodScanResult)
        {
            IEnumerable<InvocationExpressionSyntax> allInvocations = currentSyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            List<InvocationAndParentsTaintedParameters> allMethodInvocations = new List<InvocationAndParentsTaintedParameters>();

            // find all invocations with same symbol info AND number of parameters
            foreach (LevelBlock block in currentLevelBlocks) // method
            {
                foreach (InvocationExpressionSyntax inv in allInvocations) //it's invocation
                {
                    if (block.MethodSymbol == semanticModel.GetSymbolInfo(inv).Symbol)
                    {
                        if (block.MethodSymbol.Parameters.Count() == inv.ArgumentList.Arguments.Count())
                        {
                            block.NumberOfCallers += 1;
                            allMethodInvocations.Add(new InvocationAndParentsTaintedParameters() { InvocationExpression = inv, TaintedMethodParameters = block.TaintedMethodParameters });
                        }
                        else
                        {
                            methodScanResult.AppendEvidence("THERE IS A CALLER OF METHOD " + block.MethodSymbol.ToString() + " BUT WITH DIFFERENT AMOUNT OF ARGUMENTS (UNABLE TO DECIDE WHICH TAINTED ARGUMENT IS WHICH)");
                            return null; //tainted method parameters are therefore unclearable
                        }
                    }
                }
            }
            return allMethodInvocations;
        }

        private void SolveSourceAreas(SyntaxTree syntaxTree, MethodScanResult methodScanResult)
        {
            foreach (SourceArea source in taintPropagationRules.SourceAreas)
                if (syntaxTree.FilePath.Contains(source.Path))
                    methodScanResult.SourceAreasLabels.Add(source.Label);
        }

        private MethodDeclarationSyntax FindMethodParent(SyntaxNode parent)
        {
            while (parent != null)
            {
                if (parent is MethodDeclarationSyntax)
                {
                    return (MethodDeclarationSyntax)parent;
                }
                parent = parent.Parent;
            }
            return null;
        }

        private MethodScanResult ScanMethod(MethodDeclarationSyntax methodSyntax)
        {
            MethodScanResult methodScanResult = globalHelper.InitialiseMethodScanResult();
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

                FollowDataFlow(methodSyntax, invocation, methodScanResult, taintedInvocation);

                for (int i = 0; i < taintedMethod.TaintedMethodParameters.Length; i++)
                {
                    taintedMethod.TaintedMethodParameters[i] += taintedInvocation.TaintedMethodParameters[i];
                }
            }

            methodScanResult.TaintedMethodParameters = taintedMethod.TaintedMethodParameters;
            methodScanResult.MethodScanResultEndTime = DateTime.Now;

            return methodScanResult;
        }

        private void FollowDataFlow(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, Tainted tainted, int[] taintedMethodParameters = null, List<SyntaxNode> visitedNodes = null, int level = 0)
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

            if (currentNode is InvocationExpressionSyntax)
                SolveInvocationExpression(rootNode, (InvocationExpressionSyntax)currentNode, result, visitedNodes, level, tainted, taintedMethodParameters);
            else if (currentNode is ObjectCreationExpressionSyntax)
                SolveObjectCreationExpression(rootNode, (ObjectCreationExpressionSyntax)currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is AssignmentExpressionSyntax)
                SolveAssignmentExpression(rootNode, (AssignmentExpressionSyntax)currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is VariableDeclaratorSyntax)
                SolveVariableDeclarator(rootNode, (VariableDeclaratorSyntax)currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is ArgumentSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is IdentifierNameSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is ConditionalExpressionSyntax)
                SolveConditionalExpression(rootNode, (ConditionalExpressionSyntax)currentNode, result, visitedNodes, level, tainted);
            else if (currentNode is LiteralExpressionSyntax)
                SolveLiteralExpression(result, level);
            else
                result.AppendEvidence(new string(' ', level * 2) + "UNRECOGNIZED NODE " + currentNode.ToString());
        }

        private void SolveInvocationExpression(MethodDeclarationSyntax rootNode, InvocationExpressionSyntax invocationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted, int[] taintedMethodParameters = null)
        {
            if (taintPropagationRules.CleaningMethods.Any(cleaningMethod => invocationNode.ToString().Contains(cleaningMethod)))
            {
                result.AppendEvidence(new string(' ', level * 2) + "OK (Cleaning method)");
                return;
            }

            if (invocationNode.ArgumentList == null)
                return;

            for (int i = 0; i < invocationNode.ArgumentList.Arguments.Count(); i++)
            {
                if (level == 1 && (taintedMethodParameters != null && taintedMethodParameters[i] == 0))
                {
                    continue;
                }
                int numberOfTaintedMethodParametersBefore = tainted.TaintedMethodParameters.Count(num => num != 0);
                FollowDataFlow(rootNode, invocationNode.ArgumentList.Arguments[i], result, tainted, null, visitedNodes, level + 1);

                // for the first invocation only
                // for the parent's tainted method parameters only
                // if following the data flow increased the number of tainted method parameters, it means that current argument of this invocationNode should be tainted
                if (level == 1 && (taintedMethodParameters == null || taintedMethodParameters[i] > 0) && numberOfTaintedMethodParametersBefore < tainted.TaintedMethodParameters.Count(num => num != 0))
                {
                    tainted.TaintedInvocationArguments[i] += 1;
                }
            }
        }

        private void SolveObjectCreationExpression(MethodDeclarationSyntax rootNode, ObjectCreationExpressionSyntax objectCreationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted)
        {
            if (objectCreationNode.ArgumentList == null)
                return;
            foreach (ArgumentSyntax argNode in objectCreationNode.ArgumentList.Arguments)
                FollowDataFlow(rootNode, argNode, result, tainted, null, visitedNodes, level + 1);
        }

        private void SolveAssignmentExpression(MethodDeclarationSyntax rootNode, AssignmentExpressionSyntax assignmentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted)
        {
            FollowDataFlow(rootNode, assignmentNode.Right, result, tainted, null, visitedNodes, level + 1);
        }

        private void SolveVariableDeclarator(MethodDeclarationSyntax rootNode, VariableDeclaratorSyntax variableDeclaratorNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted)
        {
            var eq = variableDeclaratorNode.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

            if (eq != null)
                foreach (var dec in eq.ChildNodes())
                    FollowDataFlow(rootNode, dec, result, tainted, null, visitedNodes, level + 1);
        }

        private void FindOrigin(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted)
        {
            string arg = currentNode.ToString();

            if (currentNode is ArgumentSyntax)
            {
                if (currentNode.ChildNodes().First() is MemberAccessExpressionSyntax)
                    arg = currentNode.ChildNodes().First().ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.Text;
            }

            InvocationExpressionSyntax invocation = currentNode.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation != null)
            {
                FollowDataFlow(rootNode, invocation, result, tainted, null, visitedNodes, level + 1);
                return;
            }

            ConditionalExpressionSyntax conditional = currentNode.DescendantNodes().OfType<ConditionalExpressionSyntax>().FirstOrDefault();
            if (conditional != null)
            {
                FollowDataFlow(rootNode, conditional, result, tainted, null, visitedNodes, level + 1);
                return;
            }

            ObjectCreationExpressionSyntax objectCreation = currentNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            if (objectCreation != null)
            {
                FollowDataFlow(rootNode, objectCreation, result, tainted, null, visitedNodes, level + 1);
                return;
            }

            foreach (AssignmentExpressionSyntax assignment in rootNode.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(a => a.Left.ToString() == arg).Reverse())
            {
                if (!visitedNodes.Contains(assignment))
                {
                    FollowDataFlow(rootNode, assignment, result, tainted, null, visitedNodes, level + 1);
                    visitedNodes.Add(assignment);
                    return;
                }
            }

            foreach (VariableDeclaratorSyntax dec in rootNode.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(d => d.Identifier.Text == arg).Reverse())
            {
                if (!visitedNodes.Contains(dec))
                {
                    FollowDataFlow(rootNode, dec, result, tainted, null, visitedNodes, level + 1);
                    visitedNodes.Add(dec);
                    return;
                }
            }

            // only after no assignment, declaration,... was found, only after that test for presence among parameters
            for (int i = 0; i < rootNode.ParameterList.Parameters.Count(); i++)
            {
                if (arg == rootNode.ParameterList.Parameters[i].Identifier.Text)
                {
                    result.AppendEvidence(new string('-', (level - 2) * 2) + "> ^^^ BAD (Parameter)");
                    result.Hits++;
                    tainted.TaintedMethodParameters[i] += 1;
                }
            }
        }

        private void SolveConditionalExpression(MethodDeclarationSyntax rootNode, ConditionalExpressionSyntax currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted)
        {
            FollowDataFlow(rootNode, currentNode.Condition, result, tainted, null, visitedNodes, level + 1);
            FollowDataFlow(rootNode, currentNode.WhenTrue, result, tainted, null, visitedNodes, level + 1);
            FollowDataFlow(rootNode, currentNode.WhenFalse, result, tainted, null, visitedNodes, level + 1);
        }

        private void SolveLiteralExpression(MethodScanResult result, int level)
        {
            result.AppendEvidence(new string(' ', level * 2) + "OK (Literal)");
        }
    }
}
