using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLInjectionAnalyzer;
using UnitTests.ExpectedDiagnostics;
using UnitTests.TaintPropagationRulesExamples;

namespace UnitTests
{
    [TestClass]
    public class SimpleAnalyzerTest
    {
        private SimpleAnalyzer simpleAnalyzer;
        private ExpectedDiagnosticsSimpleAnalysis expectedDiagnosticsCreator;
        private TaintPropagationRulesCreator taintPropagationRulesCreator;
        private AnalyzerTestHelper testHelper;

        public SimpleAnalyzerTest()
        {
            simpleAnalyzer = new SimpleAnalyzer();
            expectedDiagnosticsCreator = new ExpectedDiagnosticsSimpleAnalysis();
            taintPropagationRulesCreator = new TaintPropagationRulesCreator();
            testHelper = new AnalyzerTestHelper(simpleAnalyzer);
        }

        // SCENARIOS for taint propagation rules:

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
        [TestMethod]
        public void TestLambdaExpressionRules()
        {


        }

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
