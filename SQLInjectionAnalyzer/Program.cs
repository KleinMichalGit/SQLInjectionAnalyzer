using System;
using ExceptionHandler;
using ExceptionHandler.ExceptionType;
using Model;
using Model.Rules;
using SQLInjectionAnalyzer.InputManager;
using SQLInjectionAnalyzer.OutputManager;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="ExceptionHandler.ExceptionType.AnalysisException">not implemented yet</exception>
        public static void Main(string[] args)
        {
            InputReader inputReader = new InputReader();
            ConfigFileReader configFileReader = new ConfigFileReader();
            ExceptionWriter exceptionHandler = new ExceptionWriter();
            Analyzer analyzer;
            try
            {
                Input input = inputReader.ProcessInput(args);
                if (input.WriteTutorialAndExit)
                {
                    exceptionHandler.WriteUsageTutorial();
                    return;
                }

                TaintPropagationRules taintPropagationRules = configFileReader.ProcessConfig(input.ConfigFilePath);

                switch (input.Scope)
                {
                    case ScopeOfAnalysis.Simple:
                        analyzer = new SimpleAnalyzer();
                        break;
                    case ScopeOfAnalysis.OneMethod:
                        analyzer = new OneMethodAnalyzer();
                        break;
                    case ScopeOfAnalysis.Interprocedural:
                        analyzer = new InterproceduralAnalyzer();
                        break;
                    default:
                        throw new AnalysisException("not implemented yet");
                }

                Diagnostics diagnostics = analyzer.ScanDirectory(input.SourceFolderPath, input.ExcludeSubpaths, taintPropagationRules, input.WriteOnConsole);

                OutputGenerator outputGenerator = new OutputGenerator(input.ExportPath);
                outputGenerator.CreateOutput(diagnostics);
            }
            catch (InvalidInputException invalidInputException)
            {
                Console.WriteLine(invalidInputException.Message);
                exceptionHandler.WriteUsageTutorial();
            }
            catch (AnalysisException analysisException)
            {
                Console.WriteLine(analysisException.Message);
                exceptionHandler.WriteAnalysisExceptionMessage();
            }
            catch (OutputGeneratorException)
            {
                exceptionHandler.WriteOutputGeneratorExceptionMessage();
            }
        }
    }
}
