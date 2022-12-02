using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SyntaxTree;

namespace Model.CSProject
{
    public class CSProjectScanResult
    {
        public DateTime CSProjectScanResultStartTime { get; set; }
        public DateTime CSProjectScanResultEndTime { get; set; }
        public TimeSpan CSProjectScanResultTotalTime { get { return CSProjectScanResultEndTime - CSProjectScanResultStartTime; } }

        public string Path { get; set; }
        public List<SyntaxTreeScanResult> SyntaxTreeScanResults { get; set; } = new List<SyntaxTreeScanResult>();
        public List<string> NamesOfAllCSFilesInsideThisCSProject { get; set; } = new List<string>();
    }
}
