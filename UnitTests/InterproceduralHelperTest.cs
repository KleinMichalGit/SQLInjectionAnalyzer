using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using ExceptionService.ExceptionType;
using InputService;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.Method;
using SQLInjectionAnalyzer;

namespace UnitTests
{
    [TestClass]
    public class InterproceduralHelperTest
    {
        private InterproceduralHelper interproceduralHelper = new InterproceduralHelper();
        private NodeFactory nodeFactory = new NodeFactory();
        private string path = "..\\..\\CodeToBeAnalysed\\TableOfRules\\TableOfRulesCodeToBeAnalysed.cs";

        [TestMethod]
        public void AllTaintVariablesAreCleanedInThisBranch()
        {
            var result = interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(new int[]{}, new int[]{});
            
            Assert.AreEqual(true, result);
            
            result = interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(new []{1, 1}, new []{0, 0});
            
            Assert.AreEqual(true, result);
            
            result = interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(new []{1, 1}, new []{50, 140});
            
            Assert.AreEqual(false, result);
            try
            {
                interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(new[] { 1, 1 }, new[] { 0 });
                Assert.Fail();
            }
            catch (AnalysisException e)
            {
                Assert.AreEqual("number of tainted method parameters and invocation arguments is incorrect!", e.Message);    
            }
        }

        [TestMethod]
        public void FindMethodParent()
        {
            //A();
            var invocationExpressionSyntax = nodeFactory.FindSyntaxNode<InvocationExpressionSyntax>(path, 0);
            var parent = interproceduralHelper.FindMethodParent(invocationExpressionSyntax);
            Assert.AreEqual(@"private void E()
        {
            A();
            B("""");
            C("""", """");
            D("""", 0, true);

            string myString;

            myString = 1 < 2 ? ""True"" : """";
            myString = 1 > 2 ? """" : ""False"";
            myString = B("""").Length > 9 ? ""True"" : ""False"";
        }", parent.ToString());

            var classDeclarationSyntax = nodeFactory.FindSyntaxNode<ClassDeclarationSyntax>(path, 0);
            parent = interproceduralHelper.FindMethodParent(classDeclarationSyntax);
            Assert.IsNull(parent);
        }

        [TestMethod]
        public void ComputeName()
        {
            var methodInfo = typeof(InterproceduralHelper).GetMethod("ComputeName", BindingFlags.NonPublic | BindingFlags.Instance);

            //empty array of arguments
            const string arg = "my.Funny.MethodName(int arg1, string arg2)";
            var name = (string)methodInfo.Invoke(interproceduralHelper, new object[] { arg });
            Assert.AreEqual("MethodName", name);
        }
        
        [TestMethod]
        public void FindAllCallersOfCurrentBlock()
        {
            var file = File.ReadAllText(path);
            var st = CSharpSyntaxTree.ParseText(file);
            var compilation = CSharpCompilation.Create("MockAssemblyName")
                .AddReferences(
                    MetadataReference.CreateFromFile(
                        typeof(object).Assembly.Location))
                .AddSyntaxTrees(st);
            var semanticModel = compilation.GetSemanticModel(st);

            var value = interproceduralHelper.FindAllCallersOfCurrentBlock(st, new List<LevelBlock>(), semanticModel,
                new MethodScanResult());
            
            
            Assert.AreEqual(0, value.Count);
            
            var methodDeclarationSyntax = st.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            
            value = interproceduralHelper.FindAllCallersOfCurrentBlock(
                st, 
                new List<LevelBlock>()
                {
                    new LevelBlock()
                    {
                        MethodSymbol = semanticModel.GetDeclaredSymbol(methodDeclarationSyntax)
                    }
                }, 
                semanticModel,
                new MethodScanResult()
                );
            
            Assert.AreEqual(1, value.Count);

        }
        
        [TestMethod]
        public void AddCaller()
        {
            var tree = new InterproceduralTree()
            {
                Id = 0,
                MethodName = "MyName",
                Callers = new List<InterproceduralTree>()
                {
                    new InterproceduralTree()
                    {
                        Id = 1,
                        MethodName = "Inner",
                        Callers = new List<InterproceduralTree>(),
                    }
                }
            };

            var caller = new InterproceduralTree()
            {
                Id = 2,
                MethodName = "IAmCaller",
                Callers = new List<InterproceduralTree>()
            };
            
            interproceduralHelper.AddCaller(1,tree, caller);

            Assert.IsTrue(tree.Callers[0].Callers[0].Id == 2);
        }
    }
}