using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Rules;
using SQLInjectionAnalyzer.Analyzers.OneMethodCSProjAnalyzer;

namespace UnitTests
{
    [TestClass]
    public class OneMethodCSProjAnalyzerTest
    {
        [TestMethod]
        public void ScanDirectory()
        {
            // is not be able to open project is MSBuild, test only number of CSProjFiles 
            OneMethodCSProjAnalyzer analyzer = new OneMethodCSProjAnalyzer();
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