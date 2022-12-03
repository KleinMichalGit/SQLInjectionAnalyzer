using System;
using System.Collections.Generic;
using System.Linq;
using ExceptionHandler.ExceptionType;
using Model;

namespace SQLInjectionAnalyzer.InputManager
{
    /// <summary>
    /// SQLInjectionAnalyzer.InputManager <c>InputReader</c> class.
    /// 
    /// <para>
    /// Reads and validates user-provided console input.
    /// 
    /// </para>
    /// <para>
    /// Contains <c>ProcessInput</c> method.
    /// </para>
    /// </summary>
    public class InputReader
    {
        /// <summary>
        /// Inputs the arguments are valid.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool InputArgumentsAreValid(string[] args)
        {
            return InputArgumentsAreUnique(args) &&
                   InputArgumentsAreRecognizable(args) &&
                   MandatoryArgumentsArePresent(args) &&
                   ScopeIsDefinedCorrectly(args) &&
                   ConfigPathIsDefinedCorrectly(args);
        }

        /// <summary>
        /// Inputs the arguments are unique.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool InputArgumentsAreUnique(string[] args)
        {
            HashSet<string> usedArguments = new HashSet<string>();
            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (!usedArguments.Add(argumentBeginning))
                {
                    Console.WriteLine("Input arguments are not unique.");
                    return false;
                }

            }
            return true;
        }

        /// <summary>
        /// Inputs the arguments are recognizable.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool InputArgumentsAreRecognizable(string[] args)
        {
            HashSet<string> recognizableArguments = new HashSet<string> { "--help", "--path", "--scope-of-analysis", "--exclude-paths", "--config", "--result", "--write-console" };

            foreach (string argument in args)
            {
                if (!recognizableArguments.Contains(argument.Split('=')[0]))
                {
                    Console.WriteLine("Input arguments are not recognizable.");
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Mandatories the arguments are present.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool MandatoryArgumentsArePresent(string[] args)
        {
            if (args.Length == 1 && args[0] == "--help") return true;

            bool sourcePathArgumentIsPresent = false;
            bool scopeArgumentIsPresent = false;
            bool exportPathArgumentIsPresent = false;
            bool configFIlePathArgumentIsPresent = false;

            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (argumentBeginning == "--path") sourcePathArgumentIsPresent = true;
                if (argumentBeginning == "--scope-of-analysis") scopeArgumentIsPresent = true;
                if (argumentBeginning == "--result") exportPathArgumentIsPresent = true;
                if (argumentBeginning == "--config") configFIlePathArgumentIsPresent = true;
                if (argumentBeginning == "--help")
                {
                    Console.WriteLine("Use --help only separately without other arguments.");
                    return false;
                }

            }

            if (sourcePathArgumentIsPresent && scopeArgumentIsPresent && exportPathArgumentIsPresent && configFIlePathArgumentIsPresent)
            {
                return true;
            }
            else
            {
                Console.WriteLine("At least one mandatory argument has not been specified. Please define --path, --scope-of-analysis, --result, and --config");
                return false;
            }
        }

        /// <summary>
        /// Scopes the is defined correctly.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool ScopeIsDefinedCorrectly(string[] args)
        {
            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (argumentBeginning == "--scope-of-analysis")
                {
                    HashSet<string> scopeValues = new HashSet<string> { "Simple", "OneMethod", "Interprocedural", "InterproceduralReachability" };
                    string scopeValue = argument.Split('=')[1];

                    if (scopeValues.Contains(scopeValue))
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Scope value is not defined correctly.");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Configurations the path is defined correctly.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool ConfigPathIsDefinedCorrectly(string[] args)
        {
            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (argumentBeginning == "--config")
                {
                    if (argument.Split('=')[1].EndsWith(".json"))
                    {
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Config file is not .json file.");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Processes the exclude substrings.
        /// </summary>
        /// <param name="substrings">The substrings.</param>
        /// <returns></returns>
        private List<string> ProcessExcludeSubstrings(string substrings)
        {
            if (substrings == "") return new List<string>();

            //create a list from comma-delimited subpaths
            return substrings.Split(',').ToList();
        }


        /// <summary>
        /// Gets the value from argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <returns></returns>
        private string GetValueFromArgument(string argument)
        {
            // get what is behind first assignment
            return argument.Split('=')[1];
        }

        /// <summary>
        /// Gets the scope from argument.
        /// </summary>
        /// <param name="argumentValue">The argument value.</param>
        /// <returns></returns>
        private ScopeOfAnalysis GetScopeFromArgument(string argumentValue)
        {
            if (argumentValue == "Simple") return ScopeOfAnalysis.Simple;
            if (argumentValue == "OneMethod") return ScopeOfAnalysis.OneMethod;
            if (argumentValue == "Interprocedural") return ScopeOfAnalysis.Interprocedural;
            return ScopeOfAnalysis.InterproceduralReachability;
        }

        //creates input from valid array of arguments (array of arguments has to be validated before)
        /// <summary>
        /// Creates the input.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private Input CreateInput(string[] args)
        {
            Input input = new Input();

            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (argumentBeginning == "--path") input.SourceFolderPath = GetValueFromArgument(argument);
                if (argumentBeginning == "--scope-of-analysis") input.Scope = GetScopeFromArgument(GetValueFromArgument(argument));
                if (argumentBeginning == "--config") input.ConfigFilePath = GetValueFromArgument(argument);
                if (argumentBeginning == "--result") input.ExportPath = GetValueFromArgument(argument);
                if (argumentBeginning == "--exclude-paths") input.ExcludeSubpaths = ProcessExcludeSubstrings(GetValueFromArgument(argument));
                if (argumentBeginning == "--write-console") input.WriteOnConsole = true;
                if (argumentBeginning == "--help") input.WriteTutorialAndExit = true;
            }

            return input;
        }

        /// <summary>
        /// Processes the input.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="ExceptionHandler.ExceptionType.InvalidInputException">Input arguments are invalid.</exception>
        public Input ProcessInput(string[] args)
        {
            if (!InputArgumentsAreValid(args))
            {
                throw new InvalidInputException("Input arguments are invalid.");
            }

            return CreateInput(args);
        }
    }
}
