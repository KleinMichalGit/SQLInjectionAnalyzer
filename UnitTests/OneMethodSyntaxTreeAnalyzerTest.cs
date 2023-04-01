using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLInjectionAnalyzer;
using UnitTests.ExpectedDiagnostics;
using UnitTests.TaintPropagationRulesExamples;

namespace UnitTests
{
    /// <summary>
    /// OneMethodSyntaxTreeAnalyzerTest for testing OneMethodSyntaxTree Scope of
    /// analysis via OneMethodSyntaxTreeAnalyzer.
    /// </summary>
    [TestClass]
    public class OneMethodSyntaxTreeAnalyzerTest
    {
        private OneMethodSyntaxTreeAnalyzer oneMethodSyntaxTreeAnalyzer = new OneMethodSyntaxTreeAnalyzer();
        private ExpectedDiagnosticsOneMethodSyntaxTreeAnalysis expectedDiagnosticsCreator = new ExpectedDiagnosticsOneMethodSyntaxTreeAnalysis();
        private TaintPropagationRulesCreator taintPropagationRulesCreator = new TaintPropagationRulesCreator();
        private ScenarioFactory scenarioFactory;

        public OneMethodSyntaxTreeAnalyzerTest()
        {
            scenarioFactory = new ScenarioFactory(oneMethodSyntaxTreeAnalyzer);
        }

        [TestMethod]
        public void TestCleaningRules()
        {
            // scenario: all arguments of sink method are cleaned
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/CleaningRules/AllArgumentsAreCleaned/",
                expectedDiagnosticsCreator.GetAllArgumentsAreCleanedDiagnostics(),
                new List<string>()
                );

            //scenario: not all arguments of sink method are cleaned
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/CleaningRules/NotAllArgumentsAreCleaned/",
                expectedDiagnosticsCreator.GetNotAllArgumentsAreCleanedDiagnostics(),
                new List<string>()
                );
        }

        /// <summary>
        /// scenario for assignment rule (multiple assignments in a row)
        /// </summary>
        [TestMethod]
        public void TestAssignmentRules()
        {
            // safe assignments
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/AssignmentRules/SafeAssignment/",
               expectedDiagnosticsCreator.GetSafeAssignmentsDiagnostics(),
               new List<string>()
               );

            //vulnerable assignments
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/AssignmentRules/VulnerableAssignment/",
               expectedDiagnosticsCreator.GetVulnerableAssignmentsDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// scenario where arguments of sink method are return values of another
        /// methods (we follow the arguments of these another methods)
        /// </summary>
        [TestMethod]
        public void TestInvocationRules()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/InvocationRules/",
                expectedDiagnosticsCreator.GetInvocationRulesDiagnostics(),
                new List<string>()
                );
        }

        /// <summary>
        /// object creation rules
        /// </summary>
        [TestMethod]
        public void TestObjectCreationRules()
        {
            // safe object creation
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/CreationRules/SafeCreationRules/",
               expectedDiagnosticsCreator.GetSafeObjectCreationDiagnostics(),
               new List<string>()
               );

            // vulnerable object creation
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/CreationRules/VulnerableCreationRules/",
               expectedDiagnosticsCreator.GetVulnerableObjectCreationDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// conditional expression rules
        /// </summary>
        [TestMethod]
        public void TestConditionalExpressionRules()
        {
            // safe conditional expression
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/ConditionalExpressionRules/SafeConditionalExpression",
               expectedDiagnosticsCreator.GetSafeConditionalExpressionDiagnostics(),
               new List<string>()
               );

            // vulnerable conditional expression
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/ConditionalExpressionRules/VulnerableConditionalExpression",
               expectedDiagnosticsCreator.GetVulnerableConditionalExpressionDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// find origin rules
        /// </summary>
        [TestMethod]
        public void TestFindOriginRules()
        {
            // safe find origin
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/FindOriginRules/SafeFindOriginRules",
               expectedDiagnosticsCreator.GetSafeFindOriginDiagnostics(),
               new List<string>()
               );

            // vulnerable find origin
            scenarioFactory.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/FindOriginRules/VulnerableFindOriginRules",
               expectedDiagnosticsCreator.GetVulnerableFindOriginDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// scenario for testing excludeSubpaths (that it really excludes these
        /// subpaths)
        /// </summary>
        [TestMethod]
        public void TestExcludingPaths()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/ExcludingPaths/",
                expectedDiagnosticsCreator.GetExcludingPathsDiagnostics(),
                new List<string>()
                {
                    "ExcludingPaths1"
                }
                );
        }

        [TestMethod]
        public void TestScanningEmptyFolder()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetEmptyRules(),
                "../../CodeToBeAnalysed/IntentionalyLeftEmptyFolder/",
                expectedDiagnosticsCreator.GetSimpleEmptyDiagnostics(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestScanningFileWithOneSafeMethod()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetEmptyRules(),
                "../../CodeToBeAnalysed/OneSafeMethod/",
                expectedDiagnosticsCreator.GetSimpleDiagnosticsWithOneCSFileScaned(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestScanningFileWithOneVulnerableMethod()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/OneVulnerableMethod/",
                expectedDiagnosticsCreator.GetOneVulnerableMethodDiagnostics(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestComplexTest()
        {
            scenarioFactory.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/Complex/",
                expectedDiagnosticsCreator.GetComplexTestDiagnostics(),
                new List<string>()
                );
        }
    }
}