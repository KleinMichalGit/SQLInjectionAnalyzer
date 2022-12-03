using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Method
{
    /// <summary>
    /// 
    /// </summary>
    public class MethodScanResult
    {
        /// <summary>
        /// Gets or sets the method scan result start time.
        /// </summary>
        /// <value>
        /// The method scan result start time.
        /// </value>
        public DateTime MethodScanResultStartTime { get; set; }
        /// <summary>
        /// Gets or sets the method scan result end time.
        /// </summary>
        /// <value>
        /// The method scan result end time.
        /// </value>
        public DateTime MethodScanResultEndTime { get; set; }
        /// <summary>
        /// Gets the method scan result total time.
        /// </summary>
        /// <value>
        /// The method scan result total time.
        /// </value>
        public TimeSpan MethodScanResultTotalTime { get { return MethodScanResultEndTime - MethodScanResultStartTime; } }

        /// <summary>
        /// The hard evidence
        /// </summary>
        private StringBuilder HardEvidence = new StringBuilder();
        /// <summary>
        /// The interprocedural callers tree
        /// </summary>
        private StringBuilder InterproceduralCallersTree = new StringBuilder();
        /// <summary>
        /// Gets or sets the sinks.
        /// </summary>
        /// <value>
        /// The sinks.
        /// </value>
        public short Sinks { get; set; } = 0;
        /// <summary>
        /// Gets or sets the hits.
        /// </summary>
        /// <value>
        /// The hits.
        /// </value>
        public short Hits { get; set; } = 0;

        /// <summary>
        /// Gets the evidence.
        /// </summary>
        /// <value>
        /// The evidence.
        /// </value>
        public string Evidence { get { return HardEvidence.ToString(); } }
        /// <summary>
        /// Gets the callers tree.
        /// </summary>
        /// <value>
        /// The callers tree.
        /// </value>
        public string CallersTree { get { return InterproceduralCallersTree.ToString(); } }

        /// <summary>
        /// Gets or sets the name of the method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public string MethodName { get; set; }
        /// <summary>
        /// Gets or sets the method body.
        /// </summary>
        /// <value>
        /// The method body.
        /// </value>
        public string MethodBody { get; set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int LineNumber { get; set; }
        /// <summary>
        /// Gets or sets the line count.
        /// </summary>
        /// <value>
        /// The line count.
        /// </value>
        public int LineCount { get; set; }
        /// <summary>
        /// The source areas labels
        /// </summary>
        public HashSet<string> SourceAreasLabels = new HashSet<string>();
        /// <summary>
        /// The bodies of callers
        /// </summary>
        public List<string> BodiesOfCallers = new List<string>();
        /// <summary>
        /// The tainted method parameters
        /// </summary>
        public int[] TaintedMethodParameters;
        /// <summary>
        /// Appends the evidence.
        /// </summary>
        /// <param name="line">The line.</param>
        public void AppendEvidence(string line)
        {
            HardEvidence.AppendLine(line);
        }
        /// <summary>
        /// Appends the caller.
        /// </summary>
        /// <param name="line">The line.</param>
        public void AppendCaller(string line)
        {
            InterproceduralCallersTree.AppendLine(line);
        }
    }
}
