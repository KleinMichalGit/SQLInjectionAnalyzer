using System.Collections.Generic;
using Model;
using Model.Rules;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Analyzer
    {
        /// <summary>
        /// Scans the directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="excludeSubpaths">The exclude subpaths.</param>
        /// <param name="taintPropagationRules">The taint propagation rules.</param>
        /// <param name="writeOnConsole">if set to <c>true</c> [write on console].</param>
        /// <returns></returns>
        public abstract Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole);
    }
}
