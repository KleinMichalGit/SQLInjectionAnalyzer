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

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>OneMethodAnalyzer</c> class.
    /// 
    /// <para>
    /// Compiles *.csproj files, without performing interprocedural analysis.
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer" />
    public class OneMethodAnalyzer : Analyzer
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

            Diagnostics diagnostics = globalHelper.InitialiseDiagnostics(ScopeOfAnalysis.OneMethodCSProj);

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
                    SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree);
                    csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                }
            }
            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = globalHelper.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

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


            if (currentNode is InvocationExpressionSyntax)
                SolveInvocationExpression(rootNode, (InvocationExpressionSyntax)currentNode, result, visitedNodes, level);
            else if (currentNode is ObjectCreationExpressionSyntax)
                SolveObjectCreationExpression(rootNode, (ObjectCreationExpressionSyntax)currentNode, result, visitedNodes, level);
            else if (currentNode is AssignmentExpressionSyntax)
                SolveAssignmentExpression(rootNode, (AssignmentExpressionSyntax)currentNode, result, visitedNodes, level);
            else if (currentNode is VariableDeclaratorSyntax)
                SolveVariableDeclarator(rootNode, (VariableDeclaratorSyntax)currentNode, result, visitedNodes, level);
            else if (currentNode is ConditionalExpressionSyntax)
                SolveConditionalExpression(rootNode, (ConditionalExpressionSyntax)currentNode, result, visitedNodes, level);
            else if (currentNode is LiteralExpressionSyntax)
                SolveLiteralExpression(result, level);
            else if (currentNode is ArgumentSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else if (currentNode is IdentifierNameSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else
                result.AppendEvidence(new string(' ', level * 2) + "UNRECOGNIZED NODE " + currentNode.ToString());
        }

        private void SolveInvocationExpression(MethodDeclarationSyntax rootNode, InvocationExpressionSyntax invocationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            if (taintPropagationRules.CleaningMethods.Any(cleaningMethod => invocationNode.ToString().Contains(cleaningMethod)))
            {
                result.AppendEvidence(new string(' ', level * 2) + "OK (Cleaning method)");
                return;
            }
            if (invocationNode.ArgumentList == null)
                return;
            foreach (ArgumentSyntax argumentNode in invocationNode.ArgumentList.Arguments)
                FollowDataFlow(rootNode, argumentNode, result, visitedNodes, level + 1);
        }

        private void SolveObjectCreationExpression(MethodDeclarationSyntax rootNode, ObjectCreationExpressionSyntax objectCreationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            if (objectCreationNode.ArgumentList == null)
                return;
            foreach (ArgumentSyntax argNode in objectCreationNode.ArgumentList.Arguments)
                FollowDataFlow(rootNode, argNode, result, visitedNodes, level + 1);
        }

        private void SolveAssignmentExpression(MethodDeclarationSyntax rootNode, AssignmentExpressionSyntax assignmentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            FollowDataFlow(rootNode, assignmentNode.Right, result, visitedNodes, level + 1);
        }

        private void SolveVariableDeclarator(MethodDeclarationSyntax rootNode, VariableDeclaratorSyntax variableDeclaratorNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            var eq = variableDeclaratorNode.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

            if (eq != null)
                foreach (var dec in eq.ChildNodes())
                    FollowDataFlow(rootNode, dec, result, visitedNodes, level + 1);
        }

        private void FindOrigin(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
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
                FollowDataFlow(rootNode, invocation, result, visitedNodes, level + 1);
                return;
            }

            ConditionalExpressionSyntax conditional = currentNode.DescendantNodes().OfType<ConditionalExpressionSyntax>().FirstOrDefault();
            if (conditional != null)
            {
                FollowDataFlow(rootNode, conditional, result, visitedNodes, level + 1);
                return;
            }

            ObjectCreationExpressionSyntax objectCreation = currentNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            if (objectCreation != null)
            {
                FollowDataFlow(rootNode, objectCreation, result, visitedNodes, level + 1);
                return;
            }

            foreach (AssignmentExpressionSyntax assignment in rootNode.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(a => a.Left.ToString() == arg).Reverse())
            {
                if (!visitedNodes.Contains(assignment))
                {
                    FollowDataFlow(rootNode, assignment, result, visitedNodes, level + 1);
                    visitedNodes.Add(assignment);
                    return;
                }
            }

            foreach (VariableDeclaratorSyntax dec in rootNode.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(d => d.Identifier.Text == arg).Reverse())
            {
                if (!visitedNodes.Contains(dec))
                {
                    FollowDataFlow(rootNode, dec, result, visitedNodes, level + 1);
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
                }
            }
        }

        private void SolveConditionalExpression(MethodDeclarationSyntax rootNode, ConditionalExpressionSyntax currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            FollowDataFlow(rootNode, currentNode.Condition, result, visitedNodes, level + 1);
            FollowDataFlow(rootNode, currentNode.WhenTrue, result, visitedNodes, level + 1);
            FollowDataFlow(rootNode, currentNode.WhenFalse, result, visitedNodes, level + 1);
        }

        private void SolveLiteralExpression(MethodScanResult result, int level)
        {
            result.AppendEvidence(new string(' ', level * 2) + "OK (Literal)");
        }
    }
}
