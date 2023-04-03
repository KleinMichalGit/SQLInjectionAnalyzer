using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLInjectionAnalyzer;

namespace UnitTests
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void MainTest()
        {
            var program = typeof(Program).Assembly.EntryPoint;
            program.Invoke(null, new object[] { new string[]{"--path=../../CodeToBeAnalysed/AssignmentRules/", "--scope-of-analysis=OneMethodSyntaxTree", "--config=../../ConfigFileExamples/ValidConfigFiles/config2.json", "--result=./", "--write-console"} });  
            if (!File.Exists("./report.html"))
            {
                Console.WriteLine("the file does not exist");
                throw new FileNotFoundException("./report.html");
            }

            var content = File.ReadAllText("./report.html");
            Assert.AreEqual(8871, content.Length);
        }
    }
}