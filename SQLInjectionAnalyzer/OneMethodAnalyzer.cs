using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
    public class OneMethodAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private string targetFileType = "*.csproj";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole = false;
        private CommonSyntaxHelper commonSyntaxHelper = new CommonSyntaxHelper();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;

            Diagnostics diagnostics = InitialiseDiagnostics(ScopeOfAnalysis.OneMethod);

            int numberOfCSProjFilesUnderThisRepository = commonSyntaxHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
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
            csprojScanResult = InitialiseScanResult(csprojPath);

            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                Project project = await workspace.OpenProjectAsync(csprojPath);

                Compilation compilation = await project.GetCompilationAsync();

                foreach (CSharpSyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                    SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
                    SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree, semanticModel);
                    csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                }

            }

            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = new SyntaxTreeScanResult();
            syntaxTreeScanResult.SyntaxTreeScanResultStartTime = DateTime.Now;
            syntaxTreeScanResult.Path = syntaxTree.FilePath;

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult)) continue;

                MethodScanResult methodScanResult = ScanMethod(methodSyntax);

                // these values are not set for method scans without hits, because it resulted into OutOfMemoryException when analysing orion monorepository
                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = semanticModel.GetDeclaredSymbol(methodSyntax).Name;
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    FileLinePositionSpan lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    if (writeOnConsole)
                    {
                        WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence);
                    }
                }
                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }


        private MethodScanResult ScanMethod(MethodDeclarationSyntax methodSyntax)
        {
            MethodScanResult methodScanResult = InitialiseMethodScanResult();

            IEnumerable<InvocationExpressionSyntax> invocations = commonSyntaxHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods);
            methodScanResult.Sinks = (short)invocations.Count();

            // follows data flow inside method for each sink invocation from sink invocation to source
            foreach (var invocation in invocations)
            {
                FollowDataFlow(methodSyntax, invocation, methodScanResult);
            }

            methodScanResult.MethodScanResultEndTime = DateTime.Now;
            return methodScanResult;
        }

        private void FollowDataFlow(SyntaxNode rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes = null, int level = 0)
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
            else if (currentNode is ArgumentSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else if (currentNode is IdentifierNameSyntax)
                FindOrigin(rootNode, currentNode, result, visitedNodes, level);
            else if (currentNode is ConditionalExpressionSyntax)
                SolveConditionalExpression(rootNode, currentNode, result, visitedNodes, level);
            else if (currentNode is LiteralExpressionSyntax)
                SolveLiteralExpression(result, level);
            else
                result.AppendEvidence(new string(' ', level * 2) + "UNRECOGNIZED NODE" + currentNode.ToString());
        }

        private void SolveInvocationExpression(SyntaxNode rootNode, InvocationExpressionSyntax invocationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
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

        private void SolveObjectCreationExpression(SyntaxNode rootNode, ObjectCreationExpressionSyntax objectCreationNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            if (objectCreationNode.ArgumentList == null)
                return;
            foreach (ArgumentSyntax argNode in objectCreationNode.ArgumentList.Arguments)
                FollowDataFlow(rootNode, argNode, result, visitedNodes, level + 1);
        }

        // follow what is behind = (everything except the first identifier)
        private void SolveAssignmentExpression(SyntaxNode rootNode, AssignmentExpressionSyntax assignmentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            var firstIdent = assignmentNode.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();

            foreach (var identifier in assignmentNode.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (identifier != firstIdent)
                    FollowDataFlow(rootNode, identifier, result, visitedNodes, level + 1);
            }
        }

        private void SolveVariableDeclarator(SyntaxNode rootNode, VariableDeclaratorSyntax variableDeclaratorNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            var eq = variableDeclaratorNode.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

            if (eq != null)
                foreach (var dec in eq.ChildNodes())
                    FollowDataFlow(rootNode, dec, result, visitedNodes, level + 1);
        }

        private void FindOrigin(SyntaxNode rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
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
                SolveInvocationExpression(rootNode, invocation, result, visitedNodes, level + 1);
                return;
            }

            ConditionalExpressionSyntax conditional = currentNode.DescendantNodes().OfType<ConditionalExpressionSyntax>().FirstOrDefault();
            if (conditional != null)
            {
                SolveConditionalExpression(rootNode, conditional, result, visitedNodes, level + 1);
                return;
            }

            ObjectCreationExpressionSyntax objectCreation = currentNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            if (objectCreation != null)
            {
                SolveObjectCreationExpression(rootNode, objectCreation, result, visitedNodes, level + 1);
                return;
            }

            bool isAssigned = false;
            foreach (AssignmentExpressionSyntax assignment in rootNode.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(a => a.Left.ToString() == arg).Reverse())
            {
                if (!visitedNodes.Contains(assignment))
                {
                    isAssigned = true;
                    FollowDataFlow(rootNode, assignment, result, visitedNodes, level + 1);
                    visitedNodes.Add(assignment);
                    break;
                }
            }

            if (!isAssigned)
            {
                foreach (VariableDeclaratorSyntax dec in rootNode.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(d => d.Identifier.Text == arg).Reverse())
                {
                    if (!visitedNodes.Contains(dec))
                    {
                        FollowDataFlow(rootNode, dec, result, visitedNodes, level + 1);
                        visitedNodes.Add(dec);
                        return;
                    }
                }
            }

            // only after no assignment, declaration,... was found, only after that test for presence among parameters
            foreach (var param in rootNode.DescendantNodes().OfType<ParameterSyntax>())
            {
                if (arg == param.Identifier.Text)
                {
                    result.AppendEvidence(new string('-', (level - 2) * 2) + "> ^^^ BAD (Parameter)");
                    result.Hits++;
                }
            }
        }

        private void SolveConditionalExpression(SyntaxNode rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level)
        {
            foreach (IdentifierNameSyntax identifier in currentNode.ChildNodes().OfType<IdentifierNameSyntax>())
            {
                result.AppendEvidence(new string(' ', level * 2) + identifier.ToString());
                FindOrigin(rootNode, identifier, result, visitedNodes, level + 1);
            }
        }

        private void SolveLiteralExpression(MethodScanResult result, int level)
        {
            result.AppendEvidence(new string(' ', level * 2) + "OK (Literal)");
        }

        private Diagnostics InitialiseDiagnostics(ScopeOfAnalysis scopeOfAnalysis)
        {
            Diagnostics diagnostics = new Diagnostics();
            diagnostics.ScopeOfAnalysis = scopeOfAnalysis;
            diagnostics.DiagnosticsStartTime = DateTime.Now;
            return diagnostics;
        }

        private CSProjectScanResult InitialiseScanResult(string directoryPath)
        {
            CSProjectScanResult scanResult = new CSProjectScanResult();
            scanResult.CSProjectScanResultStartTime = DateTime.Now;
            scanResult.Path = directoryPath;

            return scanResult;
        }

        private MethodScanResult InitialiseMethodScanResult()
        {
            MethodScanResult methodScanResult = new MethodScanResult();
            methodScanResult.MethodScanResultStartTime = DateTime.Now;

            return methodScanResult;
        }

        private bool MethodShouldBeAnalysed(MethodDeclarationSyntax methodSyntax, SyntaxTreeScanResult syntaxTreeScanResult)
        {
            //scan public methods only (will be removed)
            if (!methodSyntax.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }

            //skontrolovat aj objekty, ktore môžu mať zanorene stringy
            if (!methodSyntax.ParameterList.ToString().Contains("string"))
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }

            IEnumerable<InvocationExpressionSyntax> invocations = commonSyntaxHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods);

            if (!invocations.Any())
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }
            return true;
        }

        private void WriteEvidenceOnConsole(string methodName, string evidence)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Vulnerable method found");
            Console.WriteLine("Method name: " + methodName);
            Console.WriteLine("Evidence:");
            Console.WriteLine(evidence);
            Console.WriteLine("-----------------------");
        }
    }
}
