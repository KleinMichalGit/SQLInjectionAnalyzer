using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Model;
using Model.CSProject;
using Model.Rules;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Model.SyntaxTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model.Method;

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


        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;
            Diagnostics diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.InterproceduralCSProj);

            int numberOfSolutionFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, targetFileType);
            int numberOfScannedSolutionFilesSoFar = 0;

            Console.WriteLine(numberOfSolutionFilesUnderThisRepository);
            foreach (string filePath in Directory.EnumerateFiles(directoryPath, targetFileType, SearchOption.AllDirectories))
            {
                // skip all blacklisted .csproj files
                if (excludeSubpaths.Any(x => filePath.Contains(x)))
                {
                    diagnostics.PathsOfSkippedCSProjects.Add(filePath);
                }
                else
                {
                    Console.WriteLine("currently scanned .sln: " + filePath);
                    Console.WriteLine(numberOfScannedSolutionFilesSoFar + " / " + numberOfSolutionFilesUnderThisRepository + " .sln files scanned");
                    ScanSolution(filePath).Wait();
                    diagnostics.CSProjectScanResults.Add(csprojScanResult);
                }

            }

            diagnostics.DiagnosticsEndTime = DateTime.Now;

            return diagnostics;
        }

        private async Task ScanSolution(string solutionPath)
        {
            using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
            {
                Solution solution = await workspace.OpenSolutionAsync(solutionPath);
                foreach(Project project in solution.Projects)
                {
                    Console.WriteLine(project.FilePath);
                    ScanCSProj(project, solution).Wait();
                }
            }
        }

        private async Task ScanCSProj(Project project, Solution solution)
        {
            csprojScanResult = diagnosticsInitializer.InitialiseScanResult(project.FilePath);
            
            Compilation compilation = await project.GetCompilationAsync();

            foreach (CSharpSyntaxTree syntaxTree in compilation.SyntaxTrees)
            {
                csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                Console.WriteLine(syntaxTree.FilePath);
                SyntaxTreeScanResult syntaxTreeScanResult = ScanSyntaxTree(syntaxTree, solution);
                csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
            }

            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree, Solution solution)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (MethodDeclarationSyntax methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules, true)) continue;

                MethodScanResult methodScanResult = InterproceduralSolutionScanMethod(methodSyntax, solution);

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

        private MethodScanResult InterproceduralSolutionScanMethod(MethodDeclarationSyntax methodSyntax, Solution solution)
        {
            MethodScanResult methodScanResult = new MethodScanResult();
            Console.WriteLine(methodSyntax.Identifier.ToString());
            return methodScanResult;
        }
    }
}
