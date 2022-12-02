using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Input
    {
        //mandatory
        public string SourceFolderPath { get; set; }
        //mandatory
        public ScopeOfAnalysis Scope { get; set; }
        //mandatory
        public string ExportPath { get; set; }
        //mandatory
        public string ConfigFilePath { get; set; }
        //optional
        public List<string> ExcludeSubpaths { get; set; } = new List<string>();
        //optional
        public bool WriteOnConsole { get; set; }
        //optional
        public bool WriteTutorialAndExit { get; set; }
    }
}
