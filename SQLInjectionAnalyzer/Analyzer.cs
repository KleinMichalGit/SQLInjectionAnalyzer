using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Rules;

namespace SQLInjectionAnalyzer
{
    public abstract class Analyzer
    {
        public abstract Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole);
    }
}
