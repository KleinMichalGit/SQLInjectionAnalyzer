using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionService.ExceptionType;
using InputService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Rules;

namespace UnitTests
{
    /// <summary>
    /// UnitTests TestClass for testing ConfigFileReader
    /// </summary>
    [TestClass]
    public class ConfigFileReaderTest
    {
        ConfigFileReader configFileReader = new ConfigFileReader();

        [TestMethod]
        public void RulesAreValid()
        {
            MethodInfo methodInfo = typeof(ConfigFileReader).GetMethod("RulesAreValid", BindingFlags.NonPublic | BindingFlags.Instance);

            // freshly initialised TaintPropagationRules are invalid because level = 0
            TaintPropagationRules rules = new TaintPropagationRules();
            bool areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { rules });
            Assert.IsFalse(areValid);

            // TaintPropagationRules where SinkMethodNames are null are invalid
            rules = new TaintPropagationRules() { Level = 5, SourceAreas = new List<SourceArea>() };
            areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { rules });
            Assert.IsFalse(areValid);

            // TaintPropagationRules where SourceAreas are null are invalid
            rules = new TaintPropagationRules() { Level = 5, SinkMethods = new List<string>() };
            areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { rules });
            Assert.IsFalse(areValid);

            //valid rules
            rules = new TaintPropagationRules() { Level = 5, SinkMethods = new List<string>(), SourceAreas = new List<SourceArea>(), CleaningMethods = new List<string>() };
            areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { rules });
            Assert.IsTrue(areValid);
        }

        [TestMethod]
        public void AllSourceAreasAreValid()
        {
            MethodInfo methodInfo = typeof(ConfigFileReader).GetMethod("AllSourceAreasAreValid", BindingFlags.NonPublic | BindingFlags.Instance);

            // valid source areas
            List<SourceArea> sourceAreas = new List<SourceArea>() { new SourceArea() { Path = "./my/path/", Label = "X" } };
            bool areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { sourceAreas });
            Assert.IsTrue(areValid);

            // invalid source areas
            sourceAreas = new List<SourceArea>() { new SourceArea() };
            areValid = (bool)methodInfo.Invoke(configFileReader, new object[] { sourceAreas });
            Assert.IsFalse(areValid);
        }

        [TestMethod]
        public void ProcessConfig()
        {
            TaintPropagationRules expectedRules = new TaintPropagationRules()
            {
                Level = 5,
                SinkMethods = new List<string> {
                    "ExecuteReader",
                    "ExecuteDataSet",
                    "ExecuteDataTable",
                    "ExecuteExists",
                    "ExecuteScalar",
                    "ExecuteNonQuery"
                },
                CleaningMethods = new List<string> {
                    "KeyValuePair",
                },
                SourceAreas = new List<SourceArea> {
                    new SourceArea()
                    {
                        Label = "X",
                        Path = "./X/Y/Z/"
                    },
                    new SourceArea()
                    {
                        Label = "A",
                        Path = "./A/B/C/"
                    },
                }
            };

            // Config file is valid
            TaintPropagationRules rules = configFileReader.ProcessConfig("../../ConfigFileExamples/ValidConfigFiles/config1.json");

            Assert.IsTrue(RulesAreEqual(expectedRules, rules));

            // Config file is invalid because level is missing
            try
            {
                configFileReader.ProcessConfig("../../ConfigFileExamples/InvalidConfigFiles/config2.json");
            }
            catch (InvalidInputException e)
            {
                Assert.AreEqual("Taint propagation rules are invalid.", e.Message);
            }

            // Config file is invalid because list of SinkMethodNames is missing
            try
            {
                configFileReader.ProcessConfig("../../ConfigFileExamples/InvalidConfigFiles/config3.json");
            }
            catch (InvalidInputException e)
            {
                Assert.AreEqual("Taint propagation rules are invalid.", e.Message);
            }
            // Config file is invalid because list of SourceAreas is missing
            try
            {
                configFileReader.ProcessConfig("../../ConfigFileExamples/InvalidConfigFiles/config4.json");
            }
            catch (InvalidInputException e)
            {
                Assert.AreEqual("Taint propagation rules are invalid.", e.Message);
            }

            // Config file does not exist under such path - should throw IOException
            try
            {
                configFileReader.ProcessConfig("../../ConfigFileExamples/InvalidConfigFiles/nonExisting.json");
            }
            catch (InvalidInputException e)
            {
                Assert.AreEqual("Can't read the config file.", e.Message);
            }
        }

        private bool RulesAreEqual(TaintPropagationRules receivedRules, TaintPropagationRules expectedRules)
        {
            if (receivedRules.Level != expectedRules.Level) return false;
            if (!Enumerable.SequenceEqual(receivedRules.SinkMethods, expectedRules.SinkMethods)) return false;
            if (!Enumerable.SequenceEqual(receivedRules.CleaningMethods, expectedRules.CleaningMethods)) return false;

            if (receivedRules.SourceAreas.Count() != expectedRules.SourceAreas.Count()) return false;

            for (int i = 0; i < receivedRules.SourceAreas.Count(); i++)
            {
                if (receivedRules.SourceAreas[i].Label != expectedRules.SourceAreas[i].Label) return false;
                if (receivedRules.SourceAreas[i].Path != expectedRules.SourceAreas[i].Path) return false;
            }

            return true;
        }
    }
}
