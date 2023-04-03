using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using Model.Method;
using OutputService;
using UnitTests.ExpectedDiagnostics;

namespace UnitTests
{
    /// <summary>
    /// UnitTests TestClass for testing ExceptionService
    /// </summary>
    [TestClass]
    public class OutputServiceTest
    {
        private Diagnostics diagnostics =
            new ExpectedDiagnosticsOneMethodSyntaxTreeAnalysis().GetComplexTestDiagnostics();
        
        
        [TestMethod]
        public void GetNumberOfAllCSProjFiles()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfAllCSProjFiles();
            Assert.AreEqual(0, value);
        }
        
        [TestMethod]
        public void GetNumberOfScannedCSProjFiles()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfScannedCSProjFiles();
            Assert.AreEqual(1, value);
        }
        
        [TestMethod]
        public void GetNumberOfSkippedCSProjFiles()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfSkippedCSProjFiles();
            Assert.AreEqual(0, value);
        }
        
        [TestMethod]
        public void GetNumberOfVulnerableMethods()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfVulnerableMethods();
            Assert.AreEqual(1, value);
        }
        
        [TestMethod]
        public void GetNumberOfVulnerableMethodsInSolution()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfVulnerableMethodsInSolution(diagnostics.SolutionScanResults[0]);
            Assert.AreEqual(1, value);
        }
        
        [TestMethod]
        public void GetNumberOfVulnerableMethodsInCSproj()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfVulnerableMethodsInCSProj(diagnostics.SolutionScanResults[0].CSProjectScanResults[0]);
            Assert.AreEqual(1, value);
        }
        
        [TestMethod]
        public void GetNumberOfVulnerableMethodsInFile()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            int value = dataExtractor.GetNumberOfVulnerableMethodsInFile(diagnostics.SolutionScanResults[0].CSProjectScanResults[0].SyntaxTreeScanResults[0]);
            Assert.AreEqual(1, value);
        }
        
        [TestMethod]
        public void ShowTree()
        {
            DataExtractor dataExtractor = new DataExtractor(diagnostics);
            string value = dataExtractor.ShowTree(
                new InterproceduralTree()
                {
                    Id = 0, 
                    MethodName = "MyMethod", 
                    Callers = new List<InterproceduralTree>()
                    {
                        new InterproceduralTree()
                        {
                            Id = 1, 
                            MethodName = "InnerMethod", 
                            Callers = new List<InterproceduralTree>()
                        }
                    }
                });
            Assert.AreEqual(@"<ul><li>
                    <details open>
                    <summary>InnerMethod</summary>
                </details>
                    </li></ul>", value);
        }

        [TestMethod]
        public void CreateHtmlOutput()
        {
            OutputGenerator outputGenerator = new OutputGenerator("./");
            
            outputGenerator.CreateOutput(diagnostics);
            if (!File.Exists("./report.html"))
            {
                Console.WriteLine("the file does not exist");
                throw new FileNotFoundException("./report.html");
            }

            string content = File.ReadAllText("./report.html");
            
            Assert.AreEqual(5889, content.Length);

            Diagnostics oneMethodDiagnostics = new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethodCSProj
            };
            outputGenerator.CreateOutput(oneMethodDiagnostics);
            if (!File.Exists("./report.html"))
            {
                Console.WriteLine("the file does not exist");
                throw new FileNotFoundException("./report.html");
            }

            content = File.ReadAllText("./report.html");
            Assert.AreEqual(4744, content.Length);
            
            Diagnostics interproceduralCSProjDiagnostics = new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.InterproceduralCSProj
            };
            outputGenerator.CreateOutput(interproceduralCSProjDiagnostics);
            if (!File.Exists("./report.html"))
            {
                Console.WriteLine("the file does not exist");
                throw new FileNotFoundException("./report.html");
            }

            content = File.ReadAllText("./report.html");
            Assert.AreEqual(5842, content.Length);
            
            Diagnostics interproceduralSolution = new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.InterproceduralSolution
            };
            outputGenerator.CreateOutput(interproceduralSolution);
            if (!File.Exists("./report.html"))
            {
                Console.WriteLine("the file does not exist");
                throw new FileNotFoundException("./report.html");
            }

            content = File.ReadAllText("./report.html");
            Assert.AreEqual(6304, content.Length);
        }
    }
}