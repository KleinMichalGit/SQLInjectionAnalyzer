﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLInjectionAnalyzer;
using UnitTests.ExpectedDiagnostics;
using UnitTests.TaintPropagationRulesExamples;

namespace UnitTests
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class SimpleAnalyzerTest
    {
        /// <summary>
        /// The simple analyzer
        /// </summary>
        private SimpleAnalyzer simpleAnalyzer;
        /// <summary>
        /// The expected diagnostics creator
        /// </summary>
        private ExpectedDiagnosticsSimpleAnalysis expectedDiagnosticsCreator;
        /// <summary>
        /// The taint propagation rules creator
        /// </summary>
        private TaintPropagationRulesCreator taintPropagationRulesCreator;
        /// <summary>
        /// The test helper
        /// </summary>
        private AnalyzerTestHelper testHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleAnalyzerTest"/> class.
        /// </summary>
        public SimpleAnalyzerTest()
        {
            simpleAnalyzer = new SimpleAnalyzer();
            expectedDiagnosticsCreator = new ExpectedDiagnosticsSimpleAnalysis();
            taintPropagationRulesCreator = new TaintPropagationRulesCreator();
            testHelper = new AnalyzerTestHelper(simpleAnalyzer);
        }

        // SCENARIOS for taint propagation rules:

        /// <summary>
        /// Tests the cleaning rules.
        /// </summary>
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

        // scenario for assignment rule (multiple assignments in a row)
        /// <summary>
        /// Tests the assignment rules.
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

        // scenario where arguments of sink method are return values of another methods (we follow the arguments of these another methods)
        /// <summary>
        /// Tests the invocation rules.
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

        // object creation rules
        /// <summary>
        /// Tests the object creation rules.
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

        // conditional expression rules
        /// <summary>
        /// Tests the conditional expression rules.
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

        // lambda expression rule
        /// <summary>
        /// Tests the lambda expression rules.
        /// </summary>
        [TestMethod]
        public void TestLambdaExpressionRules()
        {


        }

        /// <summary>
        /// Tests the excluding paths.
        /// </summary>
        [TestMethod]
        public void TestExcludingPaths()
        {
            // scenario for testing excludeSubpaths (that it really excludes these subpaths)
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

        // basic corner case tests

        /// <summary>
        /// Tests the scanning empty folder.
        /// </summary>
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

        /// <summary>
        /// Tests the scanning file with one safe method.
        /// </summary>
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

        /// <summary>
        /// Tests the scanning file with one vulnerable method.
        /// </summary>
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

        /// <summary>
        /// Tests the complex test.
        /// </summary>
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
