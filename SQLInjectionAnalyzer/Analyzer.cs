using System.Collections.Generic;
using Model;
using Model.Rules;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>Analyzer</c> class.
    /// <para>
    /// Public abstract class which has to be inherited by every single
    /// analyzer. It contains only one ScanDirectory method, which takes
    /// information received on input and in config file, and returns
    /// Diagnostics object. The way how each analyzer implements this method is
    /// completely up to a derived class.
    /// </para>
    /// <para>
    /// Contains abstract <c>ScanDirectory</c> method.
    /// </para>
    /// </summary>
    public abstract class Analyzer
    {
        /// <summary>
        /// Scans the directory.
        /// </summary>
        /// <param name="directoryPath">The directory path to be analysed.
        ///     </param>
        /// <param name="excludeSubpaths">The list of sub-paths to be skipped
        ///     during the analysis.</param>
        /// <param name="taintPropagationRules">The taint propagation rules.
        ///     </param>
        /// <param name="writeOnConsole">If set to true, write progress and
        ///     results on console in real-time.</param>
        /// <returns></returns>
        public abstract Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole);
    }
}