using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace UnitTests.ExpectedDiagnostics
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpectedDiagnosticsOneMethodAnalysis
    {
        /// <summary>
        /// Gets the one method empty diagnostics.
        /// </summary>
        /// <returns></returns>
        public Diagnostics GetOneMethodEmptyDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }

        /// <summary>
        /// Gets all arguments are cleaned diagnostics.
        /// </summary>
        /// <returns></returns>
        internal Diagnostics GetAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }

        /// <summary>
        /// Gets the not all arguments are cleaned diagnostics.
        /// </summary>
        /// <returns></returns>
        internal Diagnostics GetNotAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethod
            };
        }
    }
}
