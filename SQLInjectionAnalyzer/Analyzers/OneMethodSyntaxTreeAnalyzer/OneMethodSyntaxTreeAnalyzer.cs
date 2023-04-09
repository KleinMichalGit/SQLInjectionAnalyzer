using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;
namespace SQLInjectionAnalyzer.Analyzers.OneMethodSyntaxTreeAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>OneMethodSyntaxTreeAnalyzer</c> class.
    /// <para>
    /// Reads C# (*.cs) files separately and investigates Syntax Trees parsed
    /// from the separate C# files, without compiling .csproj files, without
    /// performing interprocedural analysis, able to decide trivial conditional
    /// statements (very fast but very inaccurate).
    /// </para>
    /// <para>
    /// Contains <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    /// <seealso cref="SQLInjectionAnalyzer.Analyzer"/>
    public class OneMethodSyntaxTreeAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;

        private const string TargetFileType = "*.cs";

        private bool writeOnConsole;
        private readonly GlobalHelper globalHelper = new GlobalHelper();
        private readonly DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        
        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;

            Diagnostics diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.OneMethodSyntaxTree);

            var numberOfCsFilesUnderThisDirectory = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, TargetFileType);
            var numberOfProcessedFiles = 0;

            var solutionScanResult = diagnosticsInitializer.InitializeSolutionScanResult(directoryPath);
            var csProjScanResult = diagnosticsInitializer.InitialiseCSProjectScanResult(directoryPath);

            foreach (var filePath in Directory.EnumerateFiles(directoryPath, TargetFileType, SearchOption.AllDirectories))
            {
                csProjScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(filePath);

                // skip scanning blacklisted files
                if (!excludeSubpaths.Any(subPath => filePath.Contains(subPath)))
                {
                    Console.WriteLine("currently scanned file: " + filePath);
                    Console.WriteLine(numberOfProcessedFiles + " / " + numberOfCsFilesUnderThisDirectory + " .cs files scanned");
                    csProjScanResult.SyntaxTreeScanResults.Add(ScanFile(filePath));
                }
                numberOfProcessedFiles++;
            }

            Console.WriteLine(numberOfProcessedFiles + " / " + numberOfCsFilesUnderThisDirectory + " .cs files scanned");

            csProjScanResult.CSProjectScanResultEndTime = DateTime.Now;

            if (csProjScanResult.SyntaxTreeScanResults.Any())
            {
                solutionScanResult.CSProjectScanResults.Add(csProjScanResult);
                diagnostics.SolutionScanResults.Add(solutionScanResult);
            }
            diagnostics.DiagnosticsEndTime = DateTime.Now;
            return diagnostics;
        }

        private SyntaxTreeScanResult ScanFile(string filePath)
        {
            var file = File.ReadAllText(filePath);
            var syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(filePath);

            var syntaxTree = CSharpSyntaxTree.ParseText(file);
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