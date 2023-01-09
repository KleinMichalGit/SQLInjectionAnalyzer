using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;
using Model;
using System.Collections.Generic;
using SQLInjectionAnalyzer.Analyzers;
using static Humanizer.In;
using System.Threading.Tasks;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>OneMethodSyntaxTreeAnalyzer</c> class.
    /// 
    /// <para>
    /// Reads C# (*.cs) files separately and investigates Syntax Trees parsed from the separate C# files,
    /// without compiling .csproj files, without performing interprocedural analysis, every block of code is
    /// considered as reachable (very fast but very inacurate).
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer" />
    public class OneMethodSyntaxTreeAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
     
        private string targetFileType = "*.cs";
        
        private bool writeOnConsole = false;
        
        private GlobalHelper globalHelper = new GlobalHelper();
        private TableOfRules tableOfRules = new TableOfRules();
        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;

            Diagnostics diagnostics = globalHelper.InitialiseDiagnostics(ScopeOfAnalysis.OneMethodSyntaxTree);

            int numberOfCSFilesUnderThisDirectory = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
            int numberOfProcessedFiles = 0;

            CSProjectScanResult scanResult = globalHelper.InitialiseScanResult(directoryPath);

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, targetFileType, SearchOption.AllDirectories))
            {
                scanResult.NamesOfAllCSFilesInsideThisCSProject.Add(filePath);

                // skip scanning blacklisted files
                if (!excludeSubpaths.Any(subPath => filePath.Contains(subPath)))
                {
                    Console.WriteLine("currently scanned file: " + filePath);
                    Console.WriteLine(numberOfProcessedFiles + " / " + numberOfCSFilesUnderThisDirectory + " .cs files scanned");
                    scanResult.SyntaxTreeScanResults.Add(ScanFile(filePath));
                }
                numberOfProcessedFiles++;
            }

            Console.WriteLine(numberOfProcessedFiles + " / " + numberOfCSFilesUnderThisDirectory + " .cs files scanned");

            scanResult.CSProjectScanResultEndTime = DateTime.Now;

            if (scanResult.SyntaxTreeScanResults.Count() > 0)
            {
                diagnostics.CSProjectScanResults.Add(scanResult);
            }
            diagnostics.DiagnosticsEndTime = DateTime.Now;
            return diagnostics;
        }

        private SyntaxTreeScanResult ScanFile(string filePath)
        {
            string file = File.ReadAllText(filePath);
            SyntaxTreeScanResult syntaxTreeScanResult = globalHelper.InitialiseSyntaxTreeScanResult(filePath);

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(file);
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("comp", syntaxTrees: new[] { syntaxTree }, references: new[] { mscorlib });

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
            MethodScanResult methodScanResult = globalHelper.InitialiseMethodScanResult();

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

            if(nextLevelNodes != null)
            {
                for(int i=0; i<nextLevelNodes.Length; i++)
                {
                    FollowDataFlow(rootNode, nextLevelNodes[i], result, visitedNodes, level);
                }
            }
        }
    }
}
