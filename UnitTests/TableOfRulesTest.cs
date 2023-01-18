using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Method;
using SQLInjectionAnalyzer.Analyzers;
using UnitTests.TaintPropagationRulesExamples;

namespace UnitTests
{
    /// <summary>
    /// TableOfRulesTest for testing each rule in TableOfRules.
    /// </summary>
    [TestClass]
    public class TableOfRulesTest
    {
        private TableOfRules tableOfRules = new TableOfRules();
        private TaintPropagationRulesCreator taintPropagationRulesCreator = new TaintPropagationRulesCreator();
        private NodeFactory nodeFactory = new NodeFactory();
        private TaintPropagationRulesCreator rules = new TaintPropagationRulesCreator();
        private string path = "..\\..\\CodeToBeAnalysed\\TableOfRules\\TableOfRulesCodeToBeAnalysed.cs";

        [TestMethod]
        public void SolveInvocationExpressionTest()
        {
            //A();
            InvocationExpressionSyntax invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path , 0);
            ArgumentSyntax[] transition = tableOfRules.SolveInvocationExpression(invocationExpressionSyntax, new MethodScanResult(), 0, rules.GetRulesWithSinkMethodNames());
            Assert.AreEqual(0, transition.Length);

            //B("");
            invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 1);
            transition = tableOfRules.SolveInvocationExpression(invocationExpressionSyntax, new MethodScanResult(), 0, rules.GetRulesWithSinkMethodNames());
            Assert.AreEqual(1, transition.Length);

            //C("", 0);
            invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 2);
            transition = tableOfRules.SolveInvocationExpression(invocationExpressionSyntax, new MethodScanResult(), 0, rules.GetRulesWithSinkMethodNames());
            Assert.AreEqual(2, transition.Length);

            //D("", 0, true);
            invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 3);
            transition = tableOfRules.SolveInvocationExpression(invocationExpressionSyntax, new MethodScanResult(), 0, rules.GetRulesWithSinkMethodNames());
            Assert.AreEqual(3, transition.Length);

            // set a rule to consider A() as a cleaning method
            MethodScanResult emptyResult = new MethodScanResult();
            invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 0);
            Assert.IsNull(tableOfRules.SolveInvocationExpression(invocationExpressionSyntax, emptyResult, 0, rules.GetRulesWithCleaningMethod()));
            Assert.AreEqual(emptyResult.Evidence, "OK (Cleaning method)\r\n");
        }

        [TestMethod]
        public void SolveObjectCreationExpressionTest()
        {
            //new MyClass();
            ObjectCreationExpressionSyntax objectCreationExpressionSyntax = nodeFactory.FindSyntaxNode<ObjectCreationExpressionSyntax>(path, 0);
            ArgumentSyntax[] transition = tableOfRules.SolveObjectCreationExpression(objectCreationExpressionSyntax);
            Assert.AreEqual(0, transition.Length);

            //new MyClass("a");
            objectCreationExpressionSyntax = nodeFactory.FindSyntaxNode<ObjectCreationExpressionSyntax>(path, 1);
            transition = tableOfRules.SolveObjectCreationExpression(objectCreationExpressionSyntax);
            Assert.AreEqual(1, transition.Length);

            //new MyClass("a", 0);
            objectCreationExpressionSyntax = nodeFactory.FindSyntaxNode<ObjectCreationExpressionSyntax>(path, 2);
            transition = tableOfRules.SolveObjectCreationExpression(objectCreationExpressionSyntax);
            Assert.AreEqual(2, transition.Length);
        }

        [TestMethod]
        public void SolveAssignmentExpressionTest()
        {
            // a = new MyClass();
            AssignmentExpressionSyntax assignmentExpressionSyntax = nodeFactory.FindSyntaxNode<AssignmentExpressionSyntax>(path, 0);
            ExpressionSyntax[] transition = tableOfRules.SolveAssignmentExpression(assignmentExpressionSyntax);
            Assert.AreEqual(transition[0].ToString(), "new MyClass()");

            // b = new MyClass("a");
            assignmentExpressionSyntax = nodeFactory.FindSyntaxNode<AssignmentExpressionSyntax>(path, 1);
            transition = tableOfRules.SolveAssignmentExpression(assignmentExpressionSyntax);
            Assert.AreEqual(transition[0].ToString(), "new MyClass(\"a\")");

            // c = new MyClass("a", 0);
            assignmentExpressionSyntax = nodeFactory.FindSyntaxNode<AssignmentExpressionSyntax>(path, 2);
            transition = tableOfRules.SolveAssignmentExpression(assignmentExpressionSyntax);
            Assert.AreEqual(transition[0].ToString(), "new MyClass(\"a\", 0)");
        }

        [TestMethod]
        public void SolveVariableDeclaratorTest()
        {
            // MyClass a = new MyClass();
            VariableDeclaratorSyntax variableDeclaratorSyntax = nodeFactory.FindSyntaxNode<VariableDeclaratorSyntax>(path, 0);
            SyntaxNode[] transition = tableOfRules.SolveVariableDeclarator(variableDeclaratorSyntax);
            Assert.AreEqual(transition[0].ToString(), "new MyClass()");

            // string myString = "my string" + " another " + "string";
            variableDeclaratorSyntax = nodeFactory.FindSyntaxNode<VariableDeclaratorSyntax>(path, 1);
            transition = tableOfRules.SolveVariableDeclarator(variableDeclaratorSyntax);
            Assert.AreEqual(transition[0].ToString(), "\"my string\" + \" another \" + \"string\"");

            // int i = -1;
            variableDeclaratorSyntax = nodeFactory.FindSyntaxNode<VariableDeclaratorSyntax>(path, 2);
            transition = tableOfRules.SolveVariableDeclarator(variableDeclaratorSyntax);
            Assert.AreEqual(transition[0].ToString(), "-1");
        }

        [TestMethod]
        public void SolveConditionalExpressionTest()
        {
            MethodScanResult emptyResult = new MethodScanResult();

            // when true
            // myString = 1 > 2 ? "True" : "";            
            ConditionalExpressionSyntax conditionalExpressionSyntax = nodeFactory.FindSyntaxNode<ConditionalExpressionSyntax>(path, 0);
            SyntaxNode[] transition = tableOfRules.SolveConditionalExpression(conditionalExpressionSyntax, emptyResult, 0).Result;
            Assert.AreEqual(transition[0].ToString(), "\"True\"");
            Assert.AreEqual(emptyResult.Evidence, "successfully evaluated condition as True (only 1 block will be investigated).\r\n");

            emptyResult = new MethodScanResult();

            //when false
            // myString = 1 < 2 ? "" : "False";
            conditionalExpressionSyntax = nodeFactory.FindSyntaxNode<ConditionalExpressionSyntax>(path, 1);
            transition = tableOfRules.SolveConditionalExpression(conditionalExpressionSyntax, emptyResult, 0).Result;
            Assert.AreEqual(transition[0].ToString(), "\"False\"");
            Assert.AreEqual(emptyResult.Evidence, "successfully evaluated condition as False (only 1 block will be investigated).\r\n");

            emptyResult = new MethodScanResult();

            //when unable to decide
            // myString = B("").Length > 9 ? "True" : "False";
            conditionalExpressionSyntax = nodeFactory.FindSyntaxNode<ConditionalExpressionSyntax>(path, 2);
            transition = tableOfRules.SolveConditionalExpression(conditionalExpressionSyntax, emptyResult, 0).Result;
            Assert.AreEqual(transition[0].ToString(), "\"True\"");
            Assert.AreEqual(transition[1].ToString(), "\"False\"");
            Assert.AreEqual(emptyResult.Evidence, "unsuccessfully evaluated condition (both blocks will be investigated).\r\n");
        }

        [TestMethod]
        public void SolveLiteralExpressionTest()
        {
            MethodScanResult emptyResult = new MethodScanResult();
           
            tableOfRules.SolveLiteralExpression(emptyResult, 0);
            Assert.AreEqual(emptyResult.Evidence, "OK (Literal)\r\n");
        }

        [TestMethod]
        public void SolveUnrecognizedSyntaxNode()
        {
            MethodScanResult emptyResult = new MethodScanResult();
           
            // consider the following syntax node as unrecognized syntax node
            ConditionalExpressionSyntax conditionalExpressionSyntax = nodeFactory.FindSyntaxNode<ConditionalExpressionSyntax>(path, 0);
            tableOfRules.SolveUnrecognizedSyntaxNode(emptyResult, conditionalExpressionSyntax, 0);
            Assert.AreEqual(emptyResult.Evidence, "UNRECOGNIZED NODE 1 < 2 ? \"True\" : \"\"\r\n");
        }

        [TestMethod]
        public void FindOriginTest()
        {
            MethodScanResult emptyResult = new MethodScanResult();

            // find the last declaration of argument
            MethodDeclarationSyntax methodDeclarationSyntax = nodeFactory.FindSyntaxNode<MethodDeclarationSyntax>(path, 5);
            InvocationExpressionSyntax invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 5);
            SyntaxNode[] transition = tableOfRules.FindOrigin(methodDeclarationSyntax, invocationExpressionSyntax.ArgumentList.Arguments[0], emptyResult, new List<SyntaxNode>(), 0);
            Assert.AreEqual(transition[0].ToString(), "c = b");

            emptyResult = new MethodScanResult();

            // find the last assignment to argument
            transition = tableOfRules.FindOrigin(methodDeclarationSyntax, invocationExpressionSyntax.ArgumentList.Arguments[1], emptyResult, new List<SyntaxNode>(), 0);
            Assert.AreEqual(transition[0].ToString(), "b = \"new string\"");

            emptyResult = new MethodScanResult();

            // vulnerability
            VariableDeclaratorSyntax declarationExpressionSyntax = nodeFactory.FindSyntaxNode<VariableDeclaratorSyntax>(path, 8);
            SyntaxNode vulnerability = tableOfRules.SolveVariableDeclarator(declarationExpressionSyntax)[0];
            Assert.IsNull(tableOfRules.FindOrigin(methodDeclarationSyntax, vulnerability, emptyResult, new List<SyntaxNode>(), 2));
            Assert.AreEqual(emptyResult.Hits, 1);
            Assert.AreEqual(emptyResult.Evidence, "> ^^^ BAD (Parameter)\r\n");
        }
    }
}
