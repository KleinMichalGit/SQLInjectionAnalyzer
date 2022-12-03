using System.Collections.Generic;
using Model;
using Model.Rules;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>Analyzer</c> class.
    /// 
    /// <para>
    /// Public abstract class which has to be inherited by every single analyzer.
    /// It contains only one ScanDirectory method, which takes information received on input and in config file, and returns Diagnostics object.
    /// The way how each analyzer implements this method is completely up to a derived class.
    /// 
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
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="excludeSubpaths">The exclude subpaths.</param>
        /// <param name="taintPropagationRules">The taint propagation rules.</param>
        /// <param name="writeOnConsole">if set to <c>true</c> [write on console].</param>
        /// <returns></returns>
        public abstract Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole);
    }
}
