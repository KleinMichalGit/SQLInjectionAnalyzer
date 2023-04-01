using System;
using System.Collections.Generic;
using Model.Method;

namespace Model.SyntaxTree
{
    /// <summary>
    /// </summary>
    public class SyntaxTreeScanResult
    {
        /// <summary>
        /// Gets or sets the syntax tree scan result start time.
        /// </summary>
        /// <value>The syntax tree scan result start time.</value>
        public DateTime SyntaxTreeScanResultStartTime { get; set; }

        /// <summary>
        /// Gets or sets the syntax tree scan result end time.
        /// </summary>
        /// <value>The syntax tree scan result end time.</value>
        public DateTime SyntaxTreeScanResultEndTime { get; set; }

        /// <summary>
        /// Gets the syntax tree scan result total time.
        /// </summary>
        /// <value>The syntax tree scan result total time.</value>
        public TimeSpan SyntaxTreeScanResultTotalTime
        { get { return SyntaxTreeScanResultEndTime - SyntaxTreeScanResultStartTime; } }

        /// <summary>
        /// Gets or sets the method scan results.
        /// </summary>
        /// <value>The method scan results.</value>
        public List<MethodScanResult> MethodScanResults { get; set; } = new List<MethodScanResult>();

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the number of skipped methods.
        /// </summary>
        /// <value>The number of skipped methods.</value>
        public int NumberOfSkippedMethods { get; set; } = 0;
    }
}