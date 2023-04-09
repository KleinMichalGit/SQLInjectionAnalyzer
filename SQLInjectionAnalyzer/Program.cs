using System;
using ExceptionService;
using ExceptionService.ExceptionType;
using InputService;
using Model;
using Model.Rules;
using OutputService;
using SQLInjectionAnalyzer.Analyzers.InterproceduralCSProjAnalyzer;
using SQLInjectionAnalyzer.Analyzers.InterproceduralSolution;
using SQLInjectionAnalyzer.Analyzers.OneMethodCSProjAnalyzer;
using SQLInjectionAnalyzer.Analyzers.OneMethodSyntaxTreeAnalyzer;

namespace SQLInjectionAnalyzer
{
    ///<summary>
    ///SQLInjectionAnalyzer <c>Program</c> class.
    ///<para>
    ///Contains <c>Main</c> method.
    ///</para>
    ///</summary>
    public class Program
    {
        ///<summary>
        ///<para>
        ///Defines the entry point of the application.
        ///</para>
        ///<para>
        ///Uses <c>InputReader</c> to process user-given input. Uses
        ///<c>ConfigFileReader</c> to process user-given config file. Uses
        ///<c>ExceptionWriter</c> to write helpful messages on console after
        ///detection of thrown unrecoverable exception. Uses <c>Analyzer</c> to
        ///analyse .csproj files under given directory and to create
        ///<c>Diagnostics</c> based on analysis results. Uses
        ///<c>OutputGenerator</c> to create report.html and report.txt files
        ///based on Scope of analysis.
        ///</para>
        ///</summary>
        ///<param name="args">array of arguments which specifies:
        ///    <list type="bullet">
        ///        <item>
        ///            --path=VALUE (MANDATORY) path to the folder which should
        ///            be analysed
        ///        </item>
        ///        <item>
        ///            --scope-of-analysis=VALUE (MANDATORY) determines the
        ///            scope of analysis
        ///        </item>
        ///        <item>
        ///            --config=VALUE (MANDATORY) path to .json config file
        ///        </item>
        ///        <item>
        ///            --result=VALUE (MANDATORY) path to the folder where
        ///            diagnostic-result-files should be exported
        ///        </item>
        ///        <item>
        ///            --exclude-paths=VALUE (OPTIONAL) comma delimited list of
        ///            sub-paths to be skipped during analysis
        ///        </item>
        ///        <item>
        ///            --write-console (OPTIONAL) write real-time
        ///            diagnostic-results on console during analysis
        ///        </item>
        ///        <item>
        ///            --help show this usage tutorial and exit
        ///        </item>
        ///    </list>
        ///</param>
        ///<exception cref="ExceptionService.ExceptionType.AnalysisException">
        ///    not implemented yet</exception>
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
                    case ScopeOfAnalysis.OneMethodSyntaxTree:
                        analyzer = new OneMethodSyntaxTreeAnalyzer();
                        break;

                    case ScopeOfAnalysis.OneMethodCSProj:
                        analyzer = new OneMethodCSProjAnalyzer();
                        break;

                    case ScopeOfAnalysis.InterproceduralCSProj:
                        analyzer = new InterproceduralCSProjAnalyzer();
                        break;

                    case ScopeOfAnalysis.InterproceduralSolution:
                        analyzer = new InterproceduralSolutionAnalyzer();
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