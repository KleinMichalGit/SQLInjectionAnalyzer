using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.OneVulnerableMethod
{
    public class SimpleClass
    {
        public void ThisIsVulnerableMethod(string args)
        {
            SinkMethodOne(args);
        }

        public void SinkMethodOne(string args)
        {

        }
    }
}
