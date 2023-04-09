using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;

namespace SQLInjectionAnalyzer.Analyzers.OneMethodCSProjAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>OneMethodCSProjAnalyzer</c> class.
    /// <para>
    /// Compiles *.csproj files, without performing interprocedural analysis.
    /// Uses the same rules as OneMethodSyntaxTree, therefore provides the same
    /// results. This ScopeOfAnalysis serves only to investigate how much time
    /// is needed for compilation of all .csproj files. Able to decide trivial
    /// conditional statements
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer"/>
    public class OneMethodCSProjAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private const string TargetFileType = "*.csproj";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole;
        private readonly GlobalHelper globalHelper = new GlobalHelper();
        private readonly DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        
        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;

            var diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.OneMethodCSProj);
            var solutionScanResult = diagnosticsInitializer.InitializeSolutionScanResult(directoryPath);

            var numberOfCsProjFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, TargetFileType);
            var numberOfScannedCsProjFilesSoFar = 0;

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, TargetFileType, SearchOption.AllDirectories))
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
                    Console.WriteLine(numberOfScannedCsProjFilesSoFar + " / " + numberOfCsProjFilesUnderThisRepository + " .csproj files scanned");
                    ScanCsProj(filePath).Wait();
                    solutionScanResult.CSProjectScanResults.Add(csprojScanResult);
                }

                numberOfScannedCsProjFilesSoFar++;
            }

            Console.WriteLine(numberOfScannedCsProjFilesSoFar + " / " + numberOfCsProjFilesUnderThisRepository + " .csproj files scanned");
            diagnostics.SolutionScanResults.Add(solutionScanResult);

            diagnostics.DiagnosticsEndTime = DateTime.Now;
            return diagnostics;
        }

        private async Task ScanCsProj(string csprojPath)
        {
            csprojScanResult = diagnosticsInitializer.InitialiseCSProjectScanResult(csprojPath);

            using (var workspace = MSBuildWorkspace.Create())
            {
                var project = await workspace.OpenProjectAsync(csprojPath);

                var compilation = await project.GetCompilationAsync();

                if (compilation != null)
                    foreach (var st in compilation.SyntaxTrees)
                    {
                        var syntaxTree = (CSharpSyntaxTree)st;
                        csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                        var syntaxTreeScanResult = ScanSyntaxTree(syntaxTree);
                        csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                    }
            }
            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree)
        {
            var syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (var methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules)) continue;

                var methodScanResult = ScanMethod(methodSyntax);

                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = methodSyntax.Identifier.ToString() + methodSyntax.ParameterList;
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    var lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    globalHelper.SolveSourceAreas(syntaxTree, methodScanResult, taintPropagationRules); // source areas labels for more informative result

                    if (writeOnConsole) globalHelper.WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence);
                }
                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }

        private MethodScanResult ScanMethod(MethodDeclarationSyntax methodSyntax)
        {
            var methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();

            var invocations = globalHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods).ToList();
            
            methodScanResult.Sinks = (short)invocations.Count();

            // follows data flow inside method for each sink invocation from sink invocation to source
            foreach (var invocation in invocations)
            {
                RuleEvaluator.EvaluateRule(methodSyntax, invocation, methodScanResult, taintPropagationRules);
            }

            methodScanResult.MethodScanResultEndTime = DateTime.Now;
            return methodScanResult;
        }
    }
}