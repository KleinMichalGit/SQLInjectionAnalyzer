using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Rules;
using SQLInjectionAnalyzer.Analyzers.InterproceduralSolution;

namespace UnitTests
{
    [TestClass]
    public class InterproceduralSolutionAnalyzerTest
    {
        [TestMethod]
        public void ScanDirectory()
        {
            // is not be able to open project is MSBuild, test only number of CSProjFiles and Solutions
            InterproceduralSolutionAnalyzer analyzer = new InterproceduralSolutionAnalyzer();
            var diag = analyzer.ScanDirectory(
                "../../../",
                new List<string>(),
                new TaintPropagationRules(){SinkMethods = new List<string>(){"SinkMethodOne"}},
                true
            );
            
            Assert.AreEqual(1, diag.NumberOfSolutions);
            Assert.AreEqual(7, diag.SolutionScanResults[0].NumberOfCSProjFiles);
        }
    }
}