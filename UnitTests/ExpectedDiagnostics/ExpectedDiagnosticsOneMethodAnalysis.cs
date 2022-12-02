using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace UnitTests.ExpectedDiagnostics
{
    public class ExpectedDiagnosticsOneMethodAnalysis
    {
        public Diagnostics GetOneMethodEmptyDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }

        internal Diagnostics GetAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }

        internal Diagnostics GetNotAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }
    }
}
