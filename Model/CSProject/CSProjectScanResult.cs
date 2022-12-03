using System;
using System.Collections.Generic;
using Model.SyntaxTree;

namespace Model.CSProject
{
    /// <summary>
    /// 
    /// </summary>
    public class CSProjectScanResult
    {
        /// <summary>
        /// Gets or sets the cs project scan result start time.
        /// </summary>
        /// <value>
        /// The cs project scan result start time.
        /// </value>
        public DateTime CSProjectScanResultStartTime { get; set; }
        /// <summary>
        /// Gets or sets the cs project scan result end time.
        /// </summary>
        /// <value>
        /// The cs project scan result end time.
        /// </value>
        public DateTime CSProjectScanResultEndTime { get; set; }
        /// <summary>
        /// Gets the cs project scan result total time.
        /// </summary>
        /// <value>
        /// The cs project scan result total time.
        /// </value>
        public TimeSpan CSProjectScanResultTotalTime { get { return CSProjectScanResultEndTime - CSProjectScanResultStartTime; } }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets the syntax tree scan results.
        /// </summary>
        /// <value>
        /// The syntax tree scan results.
        /// </value>
        public List<SyntaxTreeScanResult> SyntaxTreeScanResults { get; set; } = new List<SyntaxTreeScanResult>();
        /// <summary>
        /// Gets or sets the names of all cs files inside this cs project.
        /// </summary>
        /// <value>
        /// The names of all cs files inside this cs project.
        /// </value>
        public List<string> NamesOfAllCSFilesInsideThisCSProject { get; set; } = new List<string>();
    }
}
