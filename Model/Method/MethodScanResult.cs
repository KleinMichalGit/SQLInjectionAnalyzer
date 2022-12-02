using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Method
{
    public class MethodScanResult
    {
        public DateTime MethodScanResultStartTime { get; set; }
        public DateTime MethodScanResultEndTime { get; set; }
        public TimeSpan MethodScanResultTotalTime { get { return MethodScanResultEndTime - MethodScanResultStartTime; } }

        private StringBuilder HardEvidence = new StringBuilder();
        private StringBuilder InterproceduralCallersTree = new StringBuilder();
        public short Sinks { get; set; } = 0;
        public short Hits { get; set; } = 0;

        public string Evidence { get { return HardEvidence.ToString(); } }
        public string CallersTree { get { return InterproceduralCallersTree.ToString(); } }

        public string MethodName { get; set; }
        public string MethodBody { get; set; }

        public int LineNumber { get; set; }
        public int LineCount { get; set; }
        public HashSet<string> SourceAreasLabels = new HashSet<string>();
        public List<string> BodiesOfCallers = new List<string>();
        public int[] TaintedMethodParameters;
        public void AppendEvidence(string line)
        {
            HardEvidence.AppendLine(line);
        }
        public void AppendCaller(string line)
        {
            InterproceduralCallersTree.AppendLine(line);
        }
    }
}
