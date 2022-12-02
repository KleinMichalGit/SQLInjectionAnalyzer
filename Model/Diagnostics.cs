using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.CSProject;

namespace Model
{
    public class Diagnostics
    {
        public ScopeOfAnalysis ScopeOfAnalysis { get; set; }
        public DateTime DiagnosticsStartTime { get; set; }
        public DateTime DiagnosticsEndTime { get; set; }
        public TimeSpan DiagnosticsTotalTime { get { return DiagnosticsEndTime - DiagnosticsStartTime; } }

        public List<CSProjectScanResult> CSProjectScanResults { get; set; } = new List<CSProjectScanResult>();

        public List<String> PathsOfSkippedCSProjects { get; set; } = new List<String>();

        public int NumberOfCSProjFiles { get; set; } = 0;
    }
}
