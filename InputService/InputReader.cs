using System;
using System.Collections.Generic;
using System.Linq;
using ExceptionService.ExceptionType;
using Model;

namespace InputService
{
    /// <summary>
    /// InputService <c>InputReader</c> class.
    /// <para>
    /// Reads and validates user-provided console input.
    /// </para>
    /// <para>
    /// Contains <c>ProcessInput</c> method.
    /// </para>
    /// </summary>
    public class InputReader
    {
        /// <summary>
        /// Input arguments are valid if all of the belong are true:
        /// <list type="bullet">
        ///     <term>Input Arguments Are Unique</term>
        ///     <term>Input Arguments Are Recognizable</term>
        ///     <term>Mandatory Arguments Are Present</term>
        ///     <term>Scope Is Defined Correctly</term>
        ///     <term>Config Path Is Defined Correctly</term>
        /// </list>
        /// </summary>
        /// <param name="args">The arguments to be validated.</param>
        /// <returns>true if the arguments are valid. Otherwise, returns false.
        ///     </returns>
        private bool InputArgumentsAreValid(string[] args)
        {
            return InputArgumentsAreUnique(args) &&
                   InputArgumentsAreRecognizable(args) &&
                   MandatoryArgumentsArePresent(args) &&
                   ScopeIsDefinedCorrectly(args) &&
                   ConfigPathIsDefinedCorrectly(args);
        }

        /// <summary>
        /// Input arguments are unique if there is no argument specified more
        /// than once.
        /// </summary>
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
        /// Input arguments are recognizable if all arguments belong to the set
        /// of recognizableArguments.
        /// </summary>
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
        /// Decides if all mandatory arguments are present.
        /// </summary>
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
        /// Decides if the scope of the analysis is defined correctly. The scope
        /// is defined correctly if it belongs to the set of possible scopes.
        /// </summary>
        private bool ScopeIsDefinedCorrectly(string[] args)
        {
            foreach (string argument in args)
            {
                string argumentBeginning = argument.Split('=')[0];
                if (argumentBeginning == "--scope-of-analysis")
                {
                    HashSet<string> scopeValues = new HashSet<string>(Enum.GetNames(typeof(ScopeOfAnalysis))); // compute all names from ScopeOfAnalysis enum
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
        /// Decides if the path to the config file is defined correctly. The
        /// path to the config file is defined correctly if the file located on
        /// the specified path exists, and ends with ".json".
        /// </summary>
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
        /// Creates the list of strings from the string of substrings delimited
        /// by comma. The list of strings received serves as the list of strings
        /// which contains the subpaths to be skipped during the later phase of
        /// the analysis.
        /// </summary>
        private List<string> ProcessExcludeSubstrings(string substrings)
        {
            if (substrings == "") return new List<string>();

            //create a list from comma-delimited subpaths
            return substrings.Split(',').ToList();
        }

        private string GetValueFromArgument(string argument)
        {
            // get what is behind first assignment
            return argument.Split('=')[1];
        }

        private ScopeOfAnalysis GetScopeFromArgument(string argumentValue)
        {
            return (ScopeOfAnalysis)Enum.Parse(typeof(ScopeOfAnalysis), argumentValue);
        }

        /// <summary>
        /// Creates Input object from valid array of arguments (array of
        /// arguments has to be validated before creating Input).
        /// </summary>
        /// <param name="args">The valid array of arguments.</param>
        /// <returns>valid Input object which contains all information received
        ///     on input.</returns>
        private Input CreateInputFromValidArguments(string[] args)
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
        /// Converts the array of string arguments into Input object which
        /// contains all information received on input. Information received on
        /// input are used during all phases of the analysis, and also when
        /// generating output.
        /// </summary>
        /// <param name="args">The array of string arguments received on input.
        ///     </param>
        /// <returns>Input object.</returns>
        /// <exception cref="ExceptionService.ExceptionType.InvalidInputException">
        ///     Input arguments are invalid.</exception>
        public Input ProcessInput(string[] args)
        {
            if (InputArgumentsAreValid(args))
            {
                return CreateInputFromValidArguments(args);
            }
            throw new InvalidInputException("Input arguments are invalid.");
        }
    }
}