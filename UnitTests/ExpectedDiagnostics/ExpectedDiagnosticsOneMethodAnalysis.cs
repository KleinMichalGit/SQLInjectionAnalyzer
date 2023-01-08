using Model;

namespace UnitTests.ExpectedDiagnostics
{
    /// <summary>
    /// common helper for creating custom expected diagnostics <see cref="Diagnostics"/>
    /// for OneMethod scope of analysis <see cref="ScopeOfAnalysis"/>.
    /// </summary>
    public class ExpectedDiagnosticsOneMethodAnalysis
    {
        public Diagnostics GetOneMethodEmptyDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethodCSProj
            };
        }

        internal Diagnostics GetAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethodCSProj
            };
        }

        internal Diagnostics GetNotAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                ScopeOfAnalysis = ScopeOfAnalysis.OneMethodCSProj
            };
        }
    }
}
