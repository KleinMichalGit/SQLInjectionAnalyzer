using System;
using System.Collections.Generic;
using System.Text;

namespace Model.Method
{
    /// <summary>
    /// Model.CSProject <c>MethodScanResult</c> class.
    /// <para>
    /// Contains all the information gained during the analysis of the specific
    /// method.
    /// </para>
    /// </summary>
    public class MethodScanResult
    {
        public DateTime MethodScanResultStartTime { get; set; }

        public DateTime MethodScanResultEndTime { get; set; }

        public TimeSpan MethodScanResultTotalTime => MethodScanResultEndTime - MethodScanResultStartTime;

        private readonly StringBuilder hardEvidence = new StringBuilder();

        public InterproceduralTree InterproceduralCallersTree { get; set; }

        public short Sinks { get; set; } = 0;

        public short Hits { get; set; } = 0;

        public string Evidence => hardEvidence.ToString();

        public string MethodName { get; set; }

        public string MethodBody { get; set; }

        public int LineNumber { get; set; }

        public int LineCount { get; set; }

        public readonly HashSet<string> SourceAreasLabels = new HashSet<string>();

        public int[] TaintedMethodParameters;

        /// <summary>
        /// Appends the evidence.
        /// </summary>
        /// <param name="line">The line.</param>
        public void AppendEvidence(string line)
        {
            hardEvidence.AppendLine(line);
        }
    }
}