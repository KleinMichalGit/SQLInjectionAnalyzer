using System;
using System.IO;
using ExceptionService;
using ExceptionService.ExceptionType;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;
using OutputService;
using SQLInjectionAnalyzer;

namespace UnitTests
{
    /// <summary>
    /// UnitTests TestClass for testing ExceptionService
    /// </summary>
    [TestClass]
    public class ExceptionServiceTest
    {
        private ExceptionWriter exceptionService = new ExceptionWriter();
        
        [TestMethod]
        public void WritesUsageTutorial()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                exceptionService.WriteUsageTutorial();

                Assert.AreEqual(exceptionService.Usage.Trim(), consoleOutput.GetOuput().Trim());
            }
        }
        
        [TestMethod]
        public void WritesAnalysisExceptionMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                exceptionService.WriteAnalysisExceptionMessage();

                Assert.AreEqual("Something went wrong while performing analysis.", consoleOutput.GetOuput().Trim());
            }
        }
        
        [TestMethod]
        public void WritesOutputGeneratorExceptionMessage()
        {
            using (var consoleOutput = new ConsoleOutput())
            {
                exceptionService.WriteOutputGeneratorExceptionMessage();

                Assert.AreEqual("Something went wrong while generating output files.", consoleOutput.GetOuput().Trim());
            }
        }
        
        [TestMethod]
        public void ExceptionsAreThrown()
        {
            InterproceduralHelper helper = new InterproceduralHelper();
            try
            {
                helper.AllTaintVariablesAreCleanedInThisBranch(new int[] { }, new int[] { 1 });
                Assert.Fail();
            }
            catch (AnalysisException e)
            {
                Assert.AreEqual("number of tainted method parameters and invocation arguments is incorrect!", e.Message);
            }

            OutputGenerator outputGenerator = new OutputGenerator("./");
            try
            {
                Diagnostics diagnostics = new Diagnostics()
                {
                    ScopeOfAnalysis = ScopeOfAnalysis.TestScope,
                };
                outputGenerator.CreateOutput(diagnostics);
                Assert.Fail();
            }
            catch (OutputGeneratorException e)
            {
                Assert.AreEqual("not implemented yet", e.Message);
            }
        }
    }
    
    public class ConsoleOutput : IDisposable
    {
        private StringWriter stringWriter;
        private TextWriter originalOutput;

        public ConsoleOutput()
        {
            stringWriter = new StringWriter();
            originalOutput = Console.Out;
            Console.SetOut(stringWriter);
        }

        public string GetOuput()
        {
            return stringWriter.ToString();
        }

        public void Dispose()
        {
            Console.SetOut(originalOutput);
            stringWriter.Dispose();
        }
    }
}