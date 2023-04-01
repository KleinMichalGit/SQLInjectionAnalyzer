using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionService.ExceptionType;
using InputService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model;

namespace UnitTests
{
    /// <summary>
    /// UnitTests TestClass for testing InputReader
    /// </summary>
    [TestClass]
    public class InputReaderTest
    {
        InputReader inputReader = new InputReader();
        string[] mandatoryArgumentsExample = new string[] { "--path=./a/b", "--scope-of-analysis=OneMethodSyntaxTree", "--config=./c/d/config.json", "--result=./x/y/z/" };
        string[] allArgumentsPresentedInUsageManual = new string[] { "--path=./a/b", "--scope-of-analysis=OneMethodSyntaxTree", "--config=./c/d/config.json", "--result=./x/y/z/", "--exclude-paths=A,B", "--write-console", "--help" };

        [TestMethod]
        public void InputArgumentsAreUnique()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("InputArgumentsAreUnique", BindingFlags.NonPublic | BindingFlags.Instance);

            //empty array of arguments
            string[] args = new string[] { };
            bool isUnique = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isUnique);

            //--help is unique
            args = new string[] { "--help" };
            isUnique = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isUnique);

            //--help --help is not unique
            args = new string[] { "--help", "--help" };
            isUnique = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(isUnique);

            //when all mandatory arguments are specified it is unique
            args = mandatoryArgumentsExample;
            isUnique = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isUnique);

            //when all mandatory arguments are specified but one of them is twice, it is not unique
            args = new string[] { "--path=./a/b", "--path=./d/", "--scope-of-analysis=InterproceduralCSProj", "--config=./c/d/config.json", "--result=./x/y/z/" };
            isUnique = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(isUnique);
        }

        [TestMethod]
        public void InputArgumentsAreRecognizable()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("InputArgumentsAreRecognizable", BindingFlags.NonPublic | BindingFlags.Instance);

            //empty array of arguments
            string[] args = new string[] { };
            bool isRecognizable = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isRecognizable);

            //all mandatory arguments are recognizable
            args = mandatoryArgumentsExample;
            isRecognizable = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isRecognizable);

            //all arguments presented in usage manual are recognizable
            args = allArgumentsPresentedInUsageManual;
            isRecognizable = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isRecognizable);

            //any other argument not presented in usage manual is not recognizable

            args = new string[] { "--any-other-argument=IsNotRecognizable" };
            isRecognizable = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(isRecognizable);

        }

        [TestMethod]
        public void MandatoryArgumentsArePresent()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("MandatoryArgumentsArePresent", BindingFlags.NonPublic | BindingFlags.Instance);

            //empty array of arguments (false, because neither --help nor mandatory arguments are present)
            string[] args = new string[] { };
            bool mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(mandatoryArgumentsArePresent);

            //--help 
            args = new string[] { "--help" };
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(mandatoryArgumentsArePresent);

            //all mandatory arguments are present
            args = mandatoryArgumentsExample;
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(mandatoryArgumentsArePresent);

            //help is mixed with another arguments
            args = allArgumentsPresentedInUsageManual;
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(mandatoryArgumentsArePresent);

            //--path is missing
            args = new string[] { "--scope-of-analysis=OneMethodSyntaxTree", "--config=./c/d/config.json", "--result=./x/y/z/" };
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(mandatoryArgumentsArePresent);

            //--path and config are missing
            args = new string[] { "--scope-of-analysis=OneMethodSyntaxTree", "--result=./x/y/z/" };
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(mandatoryArgumentsArePresent);

            //--path, config, and result are missing
            args = new string[] { "--scope-of-analysis=OneMethodSyntaxTree" };
            mandatoryArgumentsArePresent = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(mandatoryArgumentsArePresent);
        }

        [TestMethod]
        public void ScopeIsDefinedCorrectly()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("ScopeIsDefinedCorrectly", BindingFlags.NonPublic | BindingFlags.Instance);

            //scope is not defined at all
            string[] args = new string[] { "--help" };
            bool scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(scopeIsDefinedCorrectly);

            //scope is defined as OneMethodSyntaxTree
            args = new string[] { "--scope-of-analysis=OneMethodSyntaxTree" };
            scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(scopeIsDefinedCorrectly);

            //scope is defined as OneMethodCSProj
            args = new string[] { "--scope-of-analysis=OneMethodCSProj" };
            scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(scopeIsDefinedCorrectly);

            //scope is defined as InterproceduralCSProj
            args = new string[] { "--scope-of-analysis=InterproceduralCSProj" };
            scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(scopeIsDefinedCorrectly);

            //scope is defined as InterproceduralSolution
            args = new string[] { "--scope-of-analysis=InterproceduralSolution" };
            scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(scopeIsDefinedCorrectly);

            //scope is not defined correctly
            args = new string[] { "--scope-of-analysis=IncorectlyDefinedScope" };
            scopeIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(scopeIsDefinedCorrectly);

        }

        [TestMethod]
        public void ConfigPathIsDefinedCorrectly()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("ConfigPathIsDefinedCorrectly", BindingFlags.NonPublic | BindingFlags.Instance);

            //config path is not defined at all
            string[] args = new string[] { "--help" };
            bool configPathIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(configPathIsDefinedCorrectly);

            //config path ends with .json
            args = new string[] { "--config=./my/path/config.json" };
            configPathIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(configPathIsDefinedCorrectly);

            //config path does not end with .json
            args = new string[] { "--config=./my/path/config.yml" };
            configPathIsDefinedCorrectly = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(configPathIsDefinedCorrectly);
        }

        [TestMethod]
        public void ProcessExcludeSubstrings()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("ProcessExcludeSubstrings", BindingFlags.NonPublic | BindingFlags.Instance);

            //empty list
            string substrings = "";
            List<string> processedSubpaths = (List<string>)methodInfo.Invoke(inputReader, new object[] { substrings });
            List<string> expected = new List<string>();
            Assert.IsTrue(Enumerable.SequenceEqual(processedSubpaths, expected));

            //one element in the list
            substrings = "A";
            processedSubpaths = (List<string>)methodInfo.Invoke(inputReader, new object[] { substrings });
            expected = new List<string> { "A" };
            Assert.IsTrue(Enumerable.SequenceEqual(processedSubpaths, expected));

            //two elements in the list
            substrings = "A,B";
            processedSubpaths = (List<string>)methodInfo.Invoke(inputReader, new object[] { substrings });
            expected = new List<string> { "A", "B" };
            Assert.IsTrue(Enumerable.SequenceEqual(processedSubpaths, expected));

            //four elements in the list
            substrings = "A,B,C,D";
            processedSubpaths = (List<string>)methodInfo.Invoke(inputReader, new object[] { substrings });
            expected = new List<string> { "A", "B", "C", "D" };
            Assert.IsTrue(Enumerable.SequenceEqual(processedSubpaths, expected));

        }

        [TestMethod]
        public void GetValueFromArgument()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("GetValueFromArgument", BindingFlags.NonPublic | BindingFlags.Instance);

            //get value from path
            string argument = "--path=./my/path/";
            string receivedValue = (string)methodInfo.Invoke(inputReader, new object[] { argument });
            string expected = "./my/path/";
            Assert.AreEqual(expected, receivedValue);

            //get value from exclude-paths
            argument = "--exclude-paths=A,B,C,D";
            receivedValue = (string)methodInfo.Invoke(inputReader, new object[] { argument });
            expected = "A,B,C,D";
            Assert.AreEqual(expected, receivedValue);
        }

        [TestMethod]
        public void GetScopeFromArgument()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("GetScopeFromArgument", BindingFlags.NonPublic | BindingFlags.Instance);

            //get OneMethodSyntaxTree Scope
            string argument = "OneMethodSyntaxTree";
            ScopeOfAnalysis receivedValue = (ScopeOfAnalysis)methodInfo.Invoke(inputReader, new object[] { argument });
            ScopeOfAnalysis expected = ScopeOfAnalysis.OneMethodSyntaxTree;
            Assert.AreEqual(expected, receivedValue);

            //get OneMethodCSProj Scope
            argument = "OneMethodCSProj";
            receivedValue = (ScopeOfAnalysis)methodInfo.Invoke(inputReader, new object[] { argument });
            expected = ScopeOfAnalysis.OneMethodCSProj;
            Assert.AreEqual(expected, receivedValue);

            //get InterproceduralCSProj Scope
            argument = "InterproceduralCSProj";
            receivedValue = (ScopeOfAnalysis)methodInfo.Invoke(inputReader, new object[] { argument });
            expected = ScopeOfAnalysis.InterproceduralCSProj;
            Assert.AreEqual(expected, receivedValue);

            //get InterproceduralSolution Scope
            argument = "InterproceduralSolution";
            receivedValue = (ScopeOfAnalysis)methodInfo.Invoke(inputReader, new object[] { argument });
            expected = ScopeOfAnalysis.InterproceduralSolution;
            Assert.AreEqual(expected, receivedValue);
        }

        [TestMethod]
        public void CreateInput()
        {
            //creates input from valid array of arguments
            MethodInfo methodInfo = typeof(InputReader).GetMethod("CreateInputFromValidArguments", BindingFlags.NonPublic | BindingFlags.Instance);

            //--help
            string[] args = new string[] { "--help" };
            Input input = (Input)methodInfo.Invoke(inputReader, new object[] { args });
            Input expected = new Input() { WriteTutorialAndExit = true };
            Assert.IsTrue(TwoInputsAreEqual(expected, input));

            //mandatory arguments
            args = mandatoryArgumentsExample;
            input = (Input)methodInfo.Invoke(inputReader, new object[] { args });
            expected = new Input()
            {
                SourceFolderPath = "./a/b",
                Scope = ScopeOfAnalysis.OneMethodSyntaxTree,
                ConfigFilePath = "./c/d/config.json",
                ExportPath = "./x/y/z/",
            };
            Assert.IsTrue(TwoInputsAreEqual(expected, input));

            //all arguments except help
            args = new string[] { "--path=./a/b", "--scope-of-analysis=InterproceduralCSProj", "--config=./c/d/config.json", "--result=./x/y/z/", "--exclude-paths=A,B", "--write-console" };
            input = (Input)methodInfo.Invoke(inputReader, new object[] { args });
            expected = new Input()
            {
                SourceFolderPath = "./a/b",
                Scope = ScopeOfAnalysis.InterproceduralCSProj,
                ConfigFilePath = "./c/d/config.json",
                ExportPath = "./x/y/z/",
                ExcludeSubpaths = new List<string> { "A", "B" },
                WriteOnConsole = true,
            };
            Assert.IsTrue(TwoInputsAreEqual(expected, input));

            //test for unequality
            args = new string[] { "--path=./a/b", "--scope-of-analysis=InterproceduralCSProj", "--config=./c/d/config.json", "--result=./x/y/z/", "--exclude-paths=A,B", "--write-console" };
            input = (Input)methodInfo.Invoke(inputReader, new object[] { args });
            expected = new Input()
            {
                ExportPath = "./x/y/z/",
                ExcludeSubpaths = new List<string> { "X" },
                WriteOnConsole = false,
            };
            Assert.IsFalse(TwoInputsAreEqual(expected, input));

        }

        [TestMethod]
        public void InputArgumentsAreValid()
        {
            MethodInfo methodInfo = typeof(InputReader).GetMethod("InputArgumentsAreValid", BindingFlags.NonPublic | BindingFlags.Instance);

            //--help is valid
            string[] args = new string[] { "--help" };
            bool isValid = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isValid);

            //mandatory arguments are valid
            args = mandatoryArgumentsExample;
            isValid = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsTrue(isValid);

            //test for invalid arguments
            args = new string[] { "--some=invalid --arguments=are --invalid" };
            isValid = (bool)methodInfo.Invoke(inputReader, new object[] { args });
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ProcessInput()
        {
            //mandatory arguments
            string[] args = mandatoryArgumentsExample;
            Input input = inputReader.ProcessInput(args);
            Input expected = new Input()
            {
                SourceFolderPath = "./a/b",
                Scope = ScopeOfAnalysis.OneMethodSyntaxTree,
                ConfigFilePath = "./c/d/config.json",
                ExportPath = "./x/y/z/",
            };

            Assert.IsTrue(TwoInputsAreEqual(expected, input));

            try
            {
                string[] invalidArgs = new string[] { "--invalid=args" };
                inputReader.ProcessInput(invalidArgs);

                Assert.Fail();
            }
            catch (InvalidInputException e)
            {
                Assert.AreEqual(e.Message, "Input arguments are invalid.");
            }
        }

        private bool TwoInputsAreEqual(Input receivedInput, Input expectedInput)
        {
            if (receivedInput.SourceFolderPath != expectedInput.SourceFolderPath) return false;
            if (receivedInput.Scope != expectedInput.Scope) return false;
            if (receivedInput.ConfigFilePath != expectedInput.ConfigFilePath) return false;
            if (receivedInput.ExportPath != expectedInput.ExportPath) return false;
            if (receivedInput.WriteOnConsole != expectedInput.WriteOnConsole) return false;
            if (receivedInput.WriteTutorialAndExit != expectedInput.WriteTutorialAndExit) return false;
            if (!Enumerable.SequenceEqual(receivedInput.ExcludeSubpaths, expectedInput.ExcludeSubpaths)) return false;

            return true;
        }
    }
}
