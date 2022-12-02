using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Method;

namespace Model.SyntaxTree
{
    public class SyntaxTreeScanResult
    {
        public DateTime SyntaxTreeScanResultStartTime { get; set; }
        public DateTime SyntaxTreeScanResultEndTime { get; set; }
        public TimeSpan SyntaxTreeScanResultTotalTime { get { return SyntaxTreeScanResultEndTime - SyntaxTreeScanResultStartTime; } }

        public List<MethodScanResult> MethodScanResults { get; set; } = new List<MethodScanResult>();

        public string Path { get; set; }

        public int NumberOfSkippedMethods { get; set; } = 0;
    }
}
