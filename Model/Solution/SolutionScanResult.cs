using Model.CSProject;
using System;
using System.Collections.Generic;

namespace Model.Solution
{
    /// <summary>
    /// Model.Solution <c>SolutionScanResult</c> class.
    /// 
    /// <para>
    /// Contains all the information gained during the analysis of the specific .sln file.
    /// </para>
    /// </summary>
    public class SolutionScanResult
    {
        public DateTime SolutionScanResultStartTime { get; set; }
        public DateTime SolutionScanResultEndTime { get; set; }
        public TimeSpan SolutionScanResultTotalTime { get { return SolutionScanResultEndTime - SolutionScanResultStartTime; } }

        /// <summary>
        /// Gets or sets the path of the analysed .sln file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the list of the separate csproj scan results. Every single dependency mentioned in the .sln
        /// has its own results stored here. The result of the analysis of csproj file is stored as a <see cref="CSProjectScanResult"/>.
        /// </summary>
        public List<CSProjectScanResult> CSProjectScanResults { get; set; } = new List<CSProjectScanResult>();

        public List<String> PathsOfSkippedCSProjects { get; set; } = new List<String>();

        /// <summary>
        /// Gets or sets the number of all .csproj files under the analysed solution.
        /// </summary>
        /// <value>
        /// The number of all .csproj files.
        /// </value>
        public int NumberOfCSProjFiles { get; set; } = 0;
    }
}
