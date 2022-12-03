using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHandler
{
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionWriter
    {
        /// <summary>
        /// Writes the usage tutorial.
        /// </summary>
        public void WriteUsageTutorial()
        {
            Console.WriteLine("Usage:\n" +
                                "\n" +
                                "--path=VALUE                 (MANDATORY) path to the folder which should be analysed\n" +
                                "--scope-of-analysis=VALUE    (MANDATORY) determines the scope of analysis\n" +
                                "--config=VALUE               (MANDATORY) path to .json config file\n" +
                                "--result=VALUE               (MANDATORY) path to the folder where diagnostic-result-files should be exported\n" +
                                "--exclude-paths=VALUE        (OPTIONAL)  comma delimited list of sub-paths to be skipped during analysis\n" +
                                "--write-console              (OPTIONAL)  write real-time diagnostic-results on console during analysis\n" +
                                "--help                                   show this usage tutorial and exit\n" +
                                "\n" +
                                "For Example:\n" +
                                "\n" +
                                ".\\SQLInjectionAnalyzer.exe --path=.\\source\\folder\\ --scope-of-analysis=Simple --config=.\\config\\folder\\config.json --result=.\\result\\path\\ --exclude-paths=TEST,E2E --write-console\n" +
                                "\n" +
                                "Parameters:\n" +
                                "\n" +
                                "--path:\n" +
                                "     any valid path to the folder which should be analysed\n" +
                                "--scope-of-analysis:\n" +
                                "     Simple                         reads *.cs files separately, without compiling .csproj files, without performing interprocedural analysis, every block of code is considered as reachable (very fast but very imprecise)\n" +
                                "     OneMethod                      compiles *.csproj files, without performing interprocedural analysis\n" +
                                "     Interprocedural                compiles *.csproj files, performs n-level interprocedural analysis, every block of code is considered as reachable\n" +
                                "     InterproceduralReachability    compiles *.csproj files, performs n-level interprocedural analysis, able to decide trivial problems when solving reachability problems (requires the most resources, the most precise type of analysis)\n" +
                                "--config:\n" +
                                "     any valid path to valid config.json (configures rules for taint propagation)\n" +
                                "--result:\n" +
                                "     any valid path to the folder where diagnostic-result-files should be exported\n" +
                                "--exclude-paths:\n" +
                                "     comma delimited list of sub-paths to be skipped during analysis (for example tests)\n" +
                                "--write-console:\n" +
                                "     informs about results in real-time\n"
                                );
        }

        /// <summary>
        /// Writes the output generator exception message.
        /// </summary>
        public void WriteOutputGeneratorExceptionMessage()
        {
            Console.WriteLine("Something went wrong while generating output files.");
        }

        /// <summary>
        /// Writes the analysis exception message.
        /// </summary>
        public void WriteAnalysisExceptionMessage()
        {
            Console.WriteLine("Something went wrong while performing analysis.");
        }
    }
}
