using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Rules;
using SQLInjectionAnalyzer.Analyzers.InterproceduralCSProjAnalyzer;

namespace UnitTests
{
    [TestClass]
    public class InterproceduralCSProjAnalyzerTest
    {
        [TestMethod]
        public void ScanDirectory()
        {
            // is not be able to open project is MSBuild, test only number of CSProjFiles 
            InterproceduralCSProjAnalyzer analyzer = new InterproceduralCSProjAnalyzer();
            var diag = analyzer.ScanDirectory(
                "../../",
                new List<string>(),
                new TaintPropagationRules(){SinkMethods = new List<string>(){"SinkMethodOne"}},
                true
            );
            
            Assert.AreEqual(1, diag.SolutionScanResults[0].NumberOfCSProjFiles);
            
        }
    }
}