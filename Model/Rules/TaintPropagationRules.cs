using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Rules
{
    public class TaintPropagationRules
    {
        public int Level { get; set; }
        public List<SourceArea> SourceAreas { get; set; }
        public List<string> SinkMethods { get; set; }
        public List<string> CleaningMethods { get; set; }
    }
}
