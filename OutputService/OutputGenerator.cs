using System;
using System.IO;
using System.Linq;
using System.Text;
using ExceptionService.ExceptionType;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Solution;
using Model.SyntaxTree;
using OutputService.RazorOutput;
using RazorEngineCore;
using RazorEngine = RazorEngineCore.RazorEngine;

namespace OutputService
{
    /// <summary>
    /// OutputService <c>OutputGenerator</c> class.
    /// <para>
    /// Class for generating .html and .txt output files according to Scope of
    /// analysis.
    /// </para>
    /// <para>
    /// Contains <c>OutputGenerator</c> constructor. Contains
    /// <c>CreateOutput</c> method. Contains <c>CreateConsoleOutput</c> method.
    /// </para>
    /// </summary>
    public class OutputGenerator
    {
        private string exportPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputGenerator"/>
        /// class.
        /// </summary>
        /// <param name="exportPath">The export path where all results of the
        ///     analysis should be created.</param>
        public OutputGenerator(string exportPath)
        {
            this.exportPath = exportPath;
        }

        /// <summary>
        /// Creates Console, HTML, and TXT output based on the Scope of analysis
        /// </summary>
        /// <param name="diagnostics">The diagnostics from which the output
        ///     should be created.</param>
        public void CreateOutput(Diagnostics diagnostics)
        {
            CreateConsoleOutput(diagnostics);
            CreateHTMLOutput(diagnostics);
            CreateTxtFileOutput(diagnostics);
        }

        /// <summary>
        /// Creates the console output.
        /// </summary>
        /// <param name="diagnostics">The diagnostics from which the output
        ///     should be created.</param>
        public void CreateConsoleOutput(Diagnostics diagnostics)
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);

            Console.WriteLine("-----------------------------");
            Console.WriteLine("Analysis start time: " + diagnostics.DiagnosticsStartTime);
            Console.WriteLine("Analysis end time: " + diagnostics.DiagnosticsEndTime);
            Console.WriteLine("Analysis total time: " + diagnostics.DiagnosticsTotalTime);
            if (diagnostics.ScopeOfAnalysis is ScopeOfAnalysis.OneMethodSyntaxTree)
            {
                Console.WriteLine("*.cs files: " + dataExtractor.GetNumberOfAllCSFiles());
            }
            else if (diagnostics.ScopeOfAnalysis is ScopeOfAnalysis.OneMethodCSProj)
            {
                Console.WriteLine("*.csproj files in directory: " + dataExtractor.GetNumberOfAllCSProjFiles());
                Console.WriteLine("Scanned *.csproj files: " + dataExtractor.GetNumberOfScannedCSProjFiles());
                Console.WriteLine("Skipped *.csproj files: " + dataExtractor.GetNumberOfSkippedCSProjFiles());
                Console.WriteLine("All *.cs files in all scanned *.csproj files: " + dataExtractor.GetNumberOfAllCSFiles());
            }

            Console.WriteLine("Scanned methods: " + dataExtractor.GetNumberOfScannedMethods());
            Console.WriteLine("Skipped methods: " + dataExtractor.GetNumberOfSkippedMethods());
            Console.WriteLine("Number of all sink invocations: " + dataExtractor.GetNumberOfAllSinks());
            Console.WriteLine("Vulnerable methods: " + dataExtractor.GetNumberOfVulnerableMethods());
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Detailed report: " + exportPath);
        }

        /// <summary>
        /// Creates the HTML output.
        /// </summary>
        /// <param name="diagnostics">The diagnostics from which the output
        ///     should be created.</param>
        /// <exception cref="ExceptionHandler.ExceptionType.OutputGeneratorException">
        ///     not implemented yet</exception>
        private void CreateHTMLOutput(Diagnostics diagnostics)
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            string content = "";


            switch (diagnostics.ScopeOfAnalysis)
            {
                case ScopeOfAnalysis.OneMethodSyntaxTree:
                    content = ReportOneMethodSyntaxTree.report;
                    break;
                case ScopeOfAnalysis.OneMethodCSProj:
                    content = ReportOneMethodCSProj.report;
                    break;
                case ScopeOfAnalysis.InterproceduralCSProj:
                    content = ReportInterproceduralCSProj.report;
                    break;
                case ScopeOfAnalysis.InterproceduralSolution:
                    content = ReportInterproceduralSolution.report;
                    break;
                default:
                    throw new OutputGeneratorException("not implemented yet");
            }

            IRazorEngineCompiledTemplate template = new RazorEngine().Compile(content);

            string result = template.Run(new
            {
                Diagnostics = diagnostics,
                NumberOfAllSolutionFiles = dataExtractor.GetNumberOfAllSolutionFiles(),
                NumberOfScannedSolutionFiles = dataExtractor.GetNumberOfScannedSolutionFiles(),
                NumberOfSkippedSolutionFiles = dataExtractor.GetNumberOfSkippedSolutionFiles(),
                NumberOfAllCSProjFiles = dataExtractor.GetNumberOfAllCSProjFiles(),
                NumberOfScannedCSProjFiles = dataExtractor.GetNumberOfScannedCSProjFiles(),
                NumberOfSkippedCSProjFiles = dataExtractor.GetNumberOfSkippedCSProjFiles(),
                NumberOfAllCSFiles = dataExtractor.GetNumberOfAllCSFiles(),
                NumberOfScannedMethods = dataExtractor.GetNumberOfScannedMethods(),
                NumberOfSkippedMethods = dataExtractor.GetNumberOfSkippedMethods(),
                NumberOfAllSinks = dataExtractor.GetNumberOfAllSinks(),
                NumberOfVulnerableMethods = dataExtractor.GetNumberOfVulnerableMethods(),
                RemoteDataExtractor = dataExtractor,
            });

            File.WriteAllText(exportPath + "\\report.html", result);
        }

        /// <summary>
        /// Creates the text file output.
        /// </summary>
        /// <param name="diagnostics">The diagnostics from which the output
        ///     should be created.</param>
        private void CreateTxtFileOutput(Diagnostics diagnostics)
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Diagnostics result:");
            sb.AppendLine("-----------------------------");
            sb.AppendLine("Analysis start time: " + diagnostics.DiagnosticsStartTime);
            sb.AppendLine("Analysis end time: " + diagnostics.DiagnosticsEndTime);
            sb.AppendLine("Analysis total time: " + diagnostics.DiagnosticsTotalTime);
            sb.AppendLine("Number of all .sln files in scanned directory: " + dataExtractor.GetNumberOfAllSolutionFiles());
            sb.AppendLine("Number of compiled .sln files: " + dataExtractor.GetNumberOfScannedSolutionFiles());
            sb.AppendLine("Number of skipped .sln files: " + dataExtractor.GetNumberOfSkippedSolutionFiles());
            sb.AppendLine("Number of all .csproj files in scanned directory: " + dataExtractor.GetNumberOfAllCSProjFiles());
            sb.AppendLine("Number of compiled .csproj files: " + dataExtractor.GetNumberOfScannedCSProjFiles());
            sb.AppendLine("Number of skipped .csproj files: " + dataExtractor.GetNumberOfSkippedCSProjFiles());
            sb.AppendLine("Number of all .cs files referred to in all compiled .csproj files: " + dataExtractor.GetNumberOfAllCSFiles());
            sb.AppendLine("Number of scanned methods: " + dataExtractor.GetNumberOfScannedMethods());
            sb.AppendLine("Number of skipped methods: " + dataExtractor.GetNumberOfSkippedMethods());
            sb.AppendLine("Number of all sink invocations: " + dataExtractor.GetNumberOfAllSinks());
            sb.AppendLine("Number of vulnerable methods: " + dataExtractor.GetNumberOfVulnerableMethods());


            if (dataExtractor.GetNumberOfVulnerableMethods() == 0)
            {
                sb.AppendLine("No vulnerabilities detected. Injection analysis passed successfully.");
                File.WriteAllText("report.txt", sb.ToString());
                return;
            }
            sb.AppendLine("List of scanned .sln files: ");

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                sb.AppendLine("List of scanned .csproj files: ");

                foreach (CSProjectScanResult csprojectScanResult in solutionScanResult.CSProjectScanResults)
                {
                    if (dataExtractor.GetNumberOfVulnerableMethodsInCSProj(csprojectScanResult) == 0)
                    {
                        continue;
                    }

                    sb.AppendLine();
                    sb.AppendLine("-----------------------------");
                    sb.AppendLine("path: " + csprojectScanResult.Path);
                    sb.AppendLine(".csproj scan start time: " + csprojectScanResult.CSProjectScanResultStartTime);
                    sb.AppendLine(".csproj scan end time: " + csprojectScanResult.CSProjectScanResultEndTime);
                    sb.AppendLine(".csproj scan total time: " + csprojectScanResult.CSProjectScanResultTotalTime);
                    sb.AppendLine("number of scanned files in this .csproj: " + csprojectScanResult.SyntaxTreeScanResults.Count());

                    sb.AppendLine();

                    sb.AppendLine("List of scanned files: ");

                    foreach (SyntaxTreeScanResult st in csprojectScanResult.SyntaxTreeScanResults)
                    {
                        if (dataExtractor.GetNumberOfVulnerableMethodsInFile(st) == 0)
                        {
                            continue;
                        }
                        sb.AppendLine("########################");
                        sb.AppendLine("path: " + st.Path);
                        sb.AppendLine("file scan start time: " + st.SyntaxTreeScanResultStartTime);
                        sb.AppendLine("file scan end time: " + st.SyntaxTreeScanResultEndTime);
                        sb.AppendLine("file scan total time: " + st.SyntaxTreeScanResultTotalTime);
                        sb.AppendLine("number of scanned methods in this file: " + st.MethodScanResults.Count());

                        sb.AppendLine();

                        sb.AppendLine("List of vulnerabilities in this file: ");

                        foreach (MethodScanResult methodScanResult in st.MethodScanResults)
                        {
                            if (methodScanResult.Hits > 0)
                            {
                                sb.AppendLine();
                                sb.AppendLine("method name: " + methodScanResult.MethodName);
                                sb.AppendLine("method implementation line number: " + methodScanResult.LineNumber);
                                sb.AppendLine("hits: " + methodScanResult.Hits + ", sinks: " + methodScanResult.Hits);
                                sb.AppendLine("method scan start time: " + methodScanResult.MethodScanResultStartTime);
                                sb.AppendLine("method scan end time: " + methodScanResult.MethodScanResultEndTime);
                                sb.AppendLine("method scan total time: " + methodScanResult.MethodScanResultTotalTime);

                                sb.AppendLine("method body: ");
                                sb.AppendLine(methodScanResult.MethodBody);

                                sb.AppendLine();
                                sb.AppendLine("evidence: ");
                                sb.AppendLine(methodScanResult.Evidence);
                            }
                        }
                    }
                }
            }

            File.WriteAllText(exportPath + "\\report.txt", sb.ToString());
        }
    }
}
