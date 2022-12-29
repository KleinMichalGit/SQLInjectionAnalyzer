using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;
using OutputService;
using SQLInjectionAnalyzer;

namespace UnitTests
{
    /// <summary>
    /// Common test helper. Contains common methods for creating test scenarios,
    /// and for testing if the received results are equal.
    /// </summary>
    public class AnalyzerTestHelper
    {
        Analyzer analyzer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyzerTestHelper"/> class.
        /// </summary>
        /// <param name="analyzer">The analyzer.</param>
        public AnalyzerTestHelper(Analyzer analyzer)
        {
            this.analyzer = analyzer;
        }

        /// <summary>
        /// Creates a new unit test scenario.
        /// </summary>
        /// <param name="rules">The rules which should be used during the scenario.</param>
        /// <param name="directoryPath">The directory path to be analysed.</param>
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
            Assert.AreEqual(expected.ScopeOfAnalysis, actual.ScopeOfAnalysis);
            // times of analyses do not have to be equal, since there is no way multiple analyses of the same code would take the same time
            // paths do not have to be equal, since they depend on the environment
            Assert.AreEqual(expected.NumberOfCSProjFiles, actual.NumberOfCSProjFiles);
            Assert.AreEqual(expected.CSProjectScanResults.Count(), actual.CSProjectScanResults.Count());

            for (int i = 0; i < expected.CSProjectScanResults.Count(); i++)
            {
                TwoCSProjectScanResultsShouldBeEqual(expected.CSProjectScanResults[i], actual.CSProjectScanResults[i]);
            }
        }

        private void TwoCSProjectScanResultsShouldBeEqual(CSProjectScanResult expected, CSProjectScanResult actual)
        {
            for (int i = 0; i < expected.NamesOfAllCSFilesInsideThisCSProject.Count(); i++)
            {
                Assert.AreEqual(expected.NamesOfAllCSFilesInsideThisCSProject[i], actual.NamesOfAllCSFilesInsideThisCSProject[i]);
            }

            Assert.AreEqual(expected.NamesOfAllCSFilesInsideThisCSProject.Count(), actual.NamesOfAllCSFilesInsideThisCSProject.Count());

            Assert.AreEqual(expected.SyntaxTreeScanResults.Count(), actual.SyntaxTreeScanResults.Count());

            for (int i = 0; i < expected.SyntaxTreeScanResults.Count(); i++)
            {
                TwoSyntaxTreeScanResultsShouldBeEqual(expected.SyntaxTreeScanResults[i], actual.SyntaxTreeScanResults[i]);
            }
        }

        private void TwoSyntaxTreeScanResultsShouldBeEqual(SyntaxTreeScanResult expected, SyntaxTreeScanResult actual)
        {
            Assert.AreEqual(expected.NumberOfSkippedMethods, actual.NumberOfSkippedMethods);
            Assert.AreEqual(expected.MethodScanResults.Count(), actual.MethodScanResults.Count());

            for (int i = 0; i < expected.MethodScanResults.Count(); i++)
            {
                TwoMethodScanResultsShouldBeEqual(expected.MethodScanResults[i], actual.MethodScanResults[i]);
            }
        }

        private void TwoMethodScanResultsShouldBeEqual(MethodScanResult expected, MethodScanResult actual)
        {

            Assert.AreEqual(expected.Sinks, actual.Sinks);
            Assert.AreEqual(expected.Hits, actual.Hits);

            // these values are not set for method scans without hits, because it resulted into OutOfMemoryException when analysing orion monorepository
            if (expected.Hits > 0)
            {
                Assert.AreEqual(expected.MethodName, actual.MethodName);
                Assert.AreEqual(expected.MethodBody, actual.MethodBody);
                Assert.AreEqual(expected.LineNumber, actual.LineNumber);
                Assert.AreEqual(expected.LineCount, actual.LineCount);
            }
        }
    }
}
