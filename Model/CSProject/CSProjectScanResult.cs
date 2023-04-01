using System;
using System.Collections.Generic;
using Model.SyntaxTree;

namespace Model.CSProject
{
    /// <summary>
    /// Model.CSProject <c>CSProjectScanResult</c> class.
    /// <para>
    /// Contains all the information gained during the analysis of the specific
    /// .csproj file.
    /// </para>
    /// </summary>
    public class CSProjectScanResult
    {
        public DateTime CSProjectScanResultStartTime { get; set; }
        public DateTime CSProjectScanResultEndTime { get; set; }
        public TimeSpan CSProjectScanResultTotalTime
        { get { return CSProjectScanResultEndTime - CSProjectScanResultStartTime; } }

        /// <summary>
        /// Gets or sets the path of the analysed .csproj file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the list of the separate syntax tree scan results.
        /// Every single dependency mentioned in the .csproj has its own results
        /// stored here. The result of the analysis of C# file is stored as a
        /// <see cref="SyntaxTreeScanResult"/>.
        /// </summary>
        public List<SyntaxTreeScanResult> SyntaxTreeScanResults { get; set; } = new List<SyntaxTreeScanResult>();

        public List<string> NamesOfAllCSFilesInsideThisCSProject { get; set; } = new List<string>();
    }
}