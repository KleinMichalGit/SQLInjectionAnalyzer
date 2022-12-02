using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExceptionHandler.ExceptionType;
using Model;

namespace SQLInjectionAnalyzer.InputManager
{
    public class InputReader
    {
        private bool InputArgumentsAreValid(string[] args)
        {
            return InputArgumentsAreUnique(args) &&
                   InputArgumentsAreRecognizable(args) &&
                   MandatoryArgumentsArePresent(args) &&
                   ScopeIsDefinedCorrectly(args) &&
                   ConfigPathIsDefinedCorrectly(args);
        }

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
            if (argumentValue == "Simple") return ScopeOfAnalysis.Simple;
            if (argumentValue == "OneMethod") return ScopeOfAnalysis.OneMethod;
            if (argumentValue == "Interprocedural") return ScopeOfAnalysis.Interprocedural;
            return ScopeOfAnalysis.InterproceduralReachability;
        }

        //creates input from valid array of arguments (array of arguments has to be validated before)
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
