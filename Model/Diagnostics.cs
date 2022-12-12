using System;
using System.Collections.Generic;
using Model.CSProject;

namespace Model
{
    /// <summary>
    /// Model <c>Diagnostics</c> class.
    /// 
    /// <para>
    /// Contains all the information gained during the analysis.
    /// </para>
    /// </summary>
    public class Diagnostics
    {
        /// <summary>
        /// Gets or sets the scope of analysis used during the analysis.
        /// </summary>
        /// <value>
        /// The scope of analysis.
        /// </value>
        public ScopeOfAnalysis ScopeOfAnalysis { get; set; }

        /// <summary>
        /// Gets or sets the diagnostics start time.
        /// </summary>
        public DateTime DiagnosticsStartTime { get; set; }

        /// <summary>
        /// Gets or sets the diagnostics end time.
        /// </summary>
        public DateTime DiagnosticsEndTime { get; set; }

        /// <summary>
        /// Gets the diagnostics total time.
        /// </summary>
        public TimeSpan DiagnosticsTotalTime { get { return DiagnosticsEndTime - DiagnosticsStartTime; } }

        /// <summary>
        /// Gets or sets the list of separate .csproject scan results. The analysis of every single .csproj file <see cref="CSProjectScanResult"/>
        /// is stored here.
        /// </summary>
        public List<CSProjectScanResult> CSProjectScanResults { get; set; } = new List<CSProjectScanResult>();

        /// <summary>
        /// Gets or sets the paths of all skipped *csproj files. For example, the .csproj file
        /// may be skipped if its file path is set to be omitted on input.
        /// </summary>
        /// <value>
        /// The paths of skipped csprojects.
        /// </value>
        public List<String> PathsOfSkippedCSProjects { get; set; } = new List<String>();

        /// <summary>
        /// Gets or sets the number of all *.csproj files under the analysed directory.
        /// </summary>
        /// <value>
        /// The number of all *.csproj files.
        /// </value>
        public int NumberOfCSProjFiles { get; set; } = 0;
    }
}
