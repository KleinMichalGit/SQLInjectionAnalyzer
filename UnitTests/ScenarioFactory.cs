using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.Solution;
using Model.SyntaxTree;
using OutputService;
using SQLInjectionAnalyzer;

namespace UnitTests
{
    /// <summary>
    /// Common test helper. Contains common methods for creating test scenarios,
    /// and for testing if the received results are equal.
    /// </summary>
    public class ScenarioFactory
    {
        Analyzer analyzer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScenarioFactory"/>
        /// class.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        public ScenarioFactory(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// Creates a new unit test scenario.
        /// </summary>
        /// <param name="rules">The rules which should be used during the
        ///     scenario.</param>
        /// <param name="directoryPath">The directory path to be analysed.
        ///     </param>
        /// <param name="expectedDiagnostics">The expected diagnostics.</param>
        /// <param name="excludeSubpaths">The exclude subpaths.</param>
        public void CreateScenario(TaintPropagationRules rules, string directoryPath, Diagnostics expectedDiagnostics, List<string> excludeSubpaths)
        {
            Diagnostics diagnostics = analyzer.ScanDirectory(directoryPath, excludeSubpaths, rules, true);
            OutputGenerator outputGenerator = new OutputGenerator("./");
            outputGenerator.CreateConsoleOutput(diagnostics);
            TwoDiagnosticFilesShouldBeEqual(expectedDiagnostics, diagnostics);
        }

        private void TwoDiagnosticFilesShouldBeEqual(Diagnostics expected, Diagnostics actual)
        {
            // times of analyses do not have to be equal, since there is no way multiple analyses of the same code would take the same time
            // paths do not have to be equal, since they depend on the environment
            expected.ScopeOfAnalysis.Should().Be(actual.ScopeOfAnalysis);
            expected.NumberOfSolutions.Should().Be(actual.NumberOfSolutions);
            expected.SolutionScanResults.Count().Should().Be(actual.SolutionScanResults.Count());

            for (int i = 0; i < expected.SolutionScanResults.Count(); i++)
            {
                TwoSolutionScanResultsShouldBeEqual(expected.SolutionScanResults[i], actual.SolutionScanResults[i]);
            }

        }

        private void TwoSolutionScanResultsShouldBeEqual(SolutionScanResult expected, SolutionScanResult actual)
        {
            expected.CSProjectScanResults.Count().Should().Be(actual.CSProjectScanResults.Count());

            for (int i = 0; i < expected.CSProjectScanResults.Count(); i++)
            {
                TwoCSProjectScanResultsShouldBeEqual(expected.CSProjectScanResults[i], actual.CSProjectScanResults[i]);
            }
        }

        private void TwoCSProjectScanResultsShouldBeEqual(CSProjectScanResult expected, CSProjectScanResult actual)
        {
            for (int i = 0; i < expected.NamesOfAllCSFilesInsideThisCSProject.Count(); i++)
            {
                expected.NamesOfAllCSFilesInsideThisCSProject[i].Should().Be(actual.NamesOfAllCSFilesInsideThisCSProject[i]);
            }

            expected.NamesOfAllCSFilesInsideThisCSProject.Count().Should().Be(actual.NamesOfAllCSFilesInsideThisCSProject.Count());
            expected.SyntaxTreeScanResults.Count().Should().Be(actual.SyntaxTreeScanResults.Count());

            for (int i = 0; i < expected.SyntaxTreeScanResults.Count(); i++)
            {
                TwoSyntaxTreeScanResultsShouldBeEqual(expected.SyntaxTreeScanResults[i], actual.SyntaxTreeScanResults[i]);
            }
        }

        private void TwoSyntaxTreeScanResultsShouldBeEqual(SyntaxTreeScanResult expected, SyntaxTreeScanResult actual)
        {
            expected.NumberOfSkippedMethods.Should().Be(actual.NumberOfSkippedMethods);
            expected.MethodScanResults.Count().Should().Be(actual.MethodScanResults.Count());

            for (int i = 0; i < expected.MethodScanResults.Count(); i++)
            {
                TwoMethodScanResultsShouldBeEqual(expected.MethodScanResults[i], actual.MethodScanResults[i]);
            }
        }

        private void TwoMethodScanResultsShouldBeEqual(MethodScanResult expected, MethodScanResult actual)
        {

            expected.Sinks.Should().Be(actual.Sinks);
            expected.Hits.Should().Be(actual.Hits);

            // these values are not set for method scans without hits, because it resulted into OutOfMemoryException when analysing orion monorepository
            if (expected.Hits > 0)
            {
                expected.MethodName.Should().Be(actual.MethodName);
                expected.MethodBody.Should().Be(actual.MethodBody);
                expected.LineNumber.Should().Be(actual.LineNumber);
                expected.LineCount.Should().Be(actual.LineCount);
            }
        }
    }
}
