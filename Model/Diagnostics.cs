using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.CSProject;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Diagnostics
    {
        /// <summary>
        /// Gets or sets the scope of analysis.
        /// </summary>
        /// <value>
        /// The scope of analysis.
        /// </value>
        public ScopeOfAnalysis ScopeOfAnalysis { get; set; }
        /// <summary>
        /// Gets or sets the diagnostics start time.
        /// </summary>
        /// <value>
        /// The diagnostics start time.
        /// </value>
        public DateTime DiagnosticsStartTime { get; set; }
        /// <summary>
        /// Gets or sets the diagnostics end time.
        /// </summary>
        /// <value>
        /// The diagnostics end time.
        /// </value>
        public DateTime DiagnosticsEndTime { get; set; }
        /// <summary>
        /// Gets the diagnostics total time.
        /// </summary>
        /// <value>
        /// The diagnostics total time.
        /// </value>
        public TimeSpan DiagnosticsTotalTime { get { return DiagnosticsEndTime - DiagnosticsStartTime; } }

        /// <summary>
        /// Gets or sets the cs project scan results.
        /// </summary>
        /// <value>
        /// The cs project scan results.
        /// </value>
        public List<CSProjectScanResult> CSProjectScanResults { get; set; } = new List<CSProjectScanResult>();

        /// <summary>
        /// Gets or sets the paths of skipped cs projects.
        /// </summary>
        /// <value>
        /// The paths of skipped cs projects.
        /// </value>
        public List<String> PathsOfSkippedCSProjects { get; set; } = new List<String>();

        /// <summary>
        /// Gets or sets the number of cs proj files.
        /// </summary>
        /// <value>
        /// The number of cs proj files.
        /// </value>
        public int NumberOfCSProjFiles { get; set; } = 0;
    }
}
