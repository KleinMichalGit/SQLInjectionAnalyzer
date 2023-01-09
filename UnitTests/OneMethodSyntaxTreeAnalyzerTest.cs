using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLInjectionAnalyzer;
using UnitTests.ExpectedDiagnostics;
using UnitTests.TaintPropagationRulesExamples;

namespace UnitTests
{
    /// <summary>
    /// UnitTests TestClass for testing OneMethodSyntaxTree scope of analysis via OneMethodSyntaxTreeAnalyzer.
    /// </summary>
    [TestClass]
    public class OneMethodSyntaxTreeAnalyzerTest
    {
        private OneMethodSyntaxTreeAnalyzer oneMethodSyntaxTreeAnalyzer;
        private ExpectedDiagnosticsOneMethodSyntaxTreeAnalysis expectedDiagnosticsCreator;
        private TaintPropagationRulesCreator taintPropagationRulesCreator;
        private AnalyzerTestHelper testHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneMethodSyntaxTreeAnalyzerTest"/> class.
        /// </summary>
        public OneMethodSyntaxTreeAnalyzerTest()
        {
            oneMethodSyntaxTreeAnalyzer = new OneMethodSyntaxTreeAnalyzer();
            expectedDiagnosticsCreator = new ExpectedDiagnosticsOneMethodSyntaxTreeAnalysis();
            taintPropagationRulesCreator = new TaintPropagationRulesCreator();
            testHelper = new AnalyzerTestHelper(oneMethodSyntaxTreeAnalyzer);
        }

        [TestMethod]
        public void TestCleaningRules()
        {
            // scenario: all arguments of sink method are cleaned
            testHelper.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/CleaningRules/AllArgumentsAreCleaned/",
                expectedDiagnosticsCreator.GetAllArgumentsAreCleanedDiagnostics(),
                new List<string>()
                );

            //scenario: not all arguments of sink method are cleaned
            testHelper.CreateScenario(
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
            testHelper.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/AssignmentRules/SafeAssignment/",
               expectedDiagnosticsCreator.GetSafeAssignmentsDiagnostics(),
               new List<string>()
               );

            //vulnerable assignments
            testHelper.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/AssignmentRules/VulnerableAssignment/",
               expectedDiagnosticsCreator.GetVulnerableAssignmentsDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// scenario where arguments of sink method are return values of
        /// another methods (we follow the arguments of these another methods)
        /// </summary>
        [TestMethod]
        public void TestInvocationRules()
        {
            testHelper.CreateScenario(
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
            testHelper.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/CreationRules/SafeCreationRules/",
               expectedDiagnosticsCreator.GetSafeObjectCreationDiagnostics(),
               new List<string>()
               );

            // vulnerable object creation
            testHelper.CreateScenario(
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
            testHelper.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/ConditionalExpressionRules/SafeConditionalExpression",
               expectedDiagnosticsCreator.GetSafeConditionalExpressionDiagnostics(),
               new List<string>()
               );

            // vulnerable conditional expression
            testHelper.CreateScenario(
               taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
               "../../CodeToBeAnalysed/ConditionalExpressionRules/VulnerableConditionalExpression",
               expectedDiagnosticsCreator.GetVulnerableConditionalExpressionDiagnostics(),
               new List<string>()
               );
        }

        /// <summary>
        /// lambda expression rule TODO
        /// </summary>
        [TestMethod]
        public void TestLambdaExpressionRules()
        {


        }

        /// <summary>
        /// scenario for testing excludeSubpaths (that it really excludes these subpaths)
        /// </summary>
        [TestMethod]
        public void TestExcludingPaths()
        {
            testHelper.CreateScenario(
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
            testHelper.CreateScenario(
                taintPropagationRulesCreator.GetEmptyRules(),
                "../../CodeToBeAnalysed/IntentionalyLeftEmptyFolder/",
                expectedDiagnosticsCreator.GetSimpleEmptyDiagnostics(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestScanningFileWithOneSafeMethod()
        {
            testHelper.CreateScenario(
                taintPropagationRulesCreator.GetEmptyRules(),
                "../../CodeToBeAnalysed/OneSafeMethod/",
                expectedDiagnosticsCreator.GetSimpleDiagnosticsWithOneCSFileScaned(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestScanningFileWithOneVulnerableMethod()
        {
            testHelper.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/OneVulnerableMethod/",
                expectedDiagnosticsCreator.GetOneVulnerableMethodDiagnostics(),
                new List<string>()
                );
        }

        [TestMethod]
        public void TestComplexTest()
        {
            testHelper.CreateScenario(
                taintPropagationRulesCreator.GetRulesWithSinkMethodNames(),
                "../../CodeToBeAnalysed/Complex/",
                expectedDiagnosticsCreator.GetComplexTestDiagnostics(),
                new List<string>()
                );
        }
    }
}
