using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;
using Model;
using Microsoft.CodeAnalysis.MSBuild;
using Humanizer;
using static Humanizer.In;
using SQLInjectionAnalyzer.Analyzers;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>OneMethodCSProjAnalyzer</c> class.
    /// 
    /// <para>
    /// Compiles *.csproj files, without performing interprocedural analysis. Uses the same rules as OneMethodSyntaxTree,
    /// therefore provides the same results. This ScopeOfAnalysis
    /// serves only to investigate how much time is needed for compilation of all .csproj files.
    /// Able to decide trivial conditional statements
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer" />
    public class OneMethodCSProjAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private string targetFileType = "*.csproj";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole = false;
        private GlobalHelper globalHelper = new GlobalHelper();
        private DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        private TableOfRules tableOfRules = new TableOfRules();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;

            Diagnostics diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.OneMethodCSProj);

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
            csprojScanResult = diagnosticsInitializer.InitialiseScanResult(csprojPath);

            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                Project project = await workspace.OpenProjectAsync(csprojPath);

                Compilation compilation = await project.GetCompilationAsync();
                

                foreach (CSharpSyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                    SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree);
                    csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                }
            }
            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules)) continue;

                MethodScanResult methodScanResult = ScanMethod(methodSyntax);

                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = methodSyntax.Identifier.ToString() + methodSyntax.ParameterList.ToString();
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    FileLinePositionSpan lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    globalHelper.SolveSourceAreas(syntaxTree, methodScanResult, taintPropagationRules); // source areas labels for more informative result

                    if (writeOnConsole)
                    {
                        globalHelper.WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence);
                    }
                }
                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }

        private MethodScanResult ScanMethod(MethodDeclarationSyntax methodSyntax)
        {
            MethodScanResult methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();

            IEnumerable<InvocationExpressionSyntax> invocations = globalHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods);
            methodScanResult.Sinks = (short)invocations.Count();

            // follows data flow inside method for each sink invocation from sink invocation to source
            foreach (var invocation in invocations)
            {
                FollowDataFlow(methodSyntax, invocation, methodScanResult);
            }

            methodScanResult.MethodScanResultEndTime = DateTime.Now;
            return methodScanResult;
        }

        private void FollowDataFlow(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes = null, int level = 0)
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
            else if (currentNode is ConditionalExpressionSyntax)
                nextLevelNodes = tableOfRules.SolveConditionalExpression((ConditionalExpressionSyntax)currentNode, result, level).Result;
            else if (currentNode is LiteralExpressionSyntax)
                tableOfRules.SolveLiteralExpression(result, level);
            else if (currentNode is ArgumentSyntax)
                nextLevelNodes = tableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else if (currentNode is IdentifierNameSyntax)
                nextLevelNodes = tableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else
                tableOfRules.SolveUnrecognizedSyntaxNode(result, currentNode, level);

            if (nextLevelNodes != null)
            {
                for (int i = 0; i < nextLevelNodes.Length; i++)
                {
                    FollowDataFlow(rootNode, nextLevelNodes[i], result, visitedNodes, level);
                }
            }
        }
    }
}
