using System;

namespace ExceptionService
{
    /// <summary>
    /// ExceptionService <c>ExceptionWriter</c> class.
    /// <para>
    /// Contains <c>WriteUsageTutorial</c> method. Contains
    /// <c>WriteOutputGeneratorExceptionMessage</c> method. Contains
    /// <c>WriteAnalysisExceptionMessage</c> method.
    /// </para>
    /// </summary>
    public class ExceptionWriter
    {
        public readonly string Usage = "Usage:\n" +
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
                                       ".\\SQLInjectionAnalyzer.exe --path=.\\source\\folder\\ --scope-of-analysis=InterproceduralCSProj --config=.\\config\\folder\\config.json --result=.\\result\\path\\ --exclude-paths=TEST,E2E --write-console\n" +
                                       "\n" +
                                       "Parameters:\n" +
                                       "\n" +
                                       "--path:\n" +
                                       "     any valid path to the folder which should be analysed\n" +
                                       "--scope-of-analysis:\n" +
                                       "     OneMethodSyntaxTree           Reads C# (*.cs) files separately and investigates Syntax Trees parsed from the separate C# files,\n" +
                                       "                                   without compiling .csproj files, without performing interprocedural analysis, able to decide trivial\n" +
                                       "                                   conditional statements (very fast but very inaccurate).\n" +
                                       "     OneMethodCSProj               Compiles *.csproj files, without performing interprocedural analysis. Uses the same rules as\n" +
                                       "                                   OneMethodSyntaxTree, therefore provides the same results. This ScopeOfAnalysis\n" +
                                       "                                   serves only to investigate how much time is needed for compilation of all .csproj files.\n" +
                                       "                                   Able to decide trivial conditional statements.\n" +
                                       "     InterproceduralCSProj         Compiles all C# project (*.csproj) files, performs n-level interprocedural analysis (where number n is defined\n" +
                                       "                                   in config.json file) for each project separately, able to decide trivial conditional statements.\n" +
                                       "     InterproceduralSolution       Opens all C# solution (*.sln) files, performs n-level interprocedural analysis (where number n is\n" +
                                       "                                   defined in config.json file) for each solution separately, able to decide trivial conditional statements.\n" +
                                       "--config:\n" +
                                       "     any valid path to valid config.json (configures rules for taint variables propagation)\n" +
                                       "--result:\n" +
                                       "     any valid path to the folder where diagnostic-result-files should be exported\n" +
                                       "--exclude-paths:\n" +
                                       "     comma delimited list of sub-paths to be skipped during analysis (for example tests)\n" +
                                       "--write-console:\n" +
                                       "     informs about results in real-time\n";
        
        /// <summary>
        /// Writes the usage tutorial.
        /// </summary>
        public void WriteUsageTutorial()
        {
            Console.WriteLine(Usage);
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