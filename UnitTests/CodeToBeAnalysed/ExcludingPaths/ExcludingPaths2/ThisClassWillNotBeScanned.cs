using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.ExcludingPaths.ExcludingPaths2
{
    public class ThisClassWillNotBeScanned
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            const string thisIsConst = "this is const";

            SinkMethodOne(thisIsConst, arg);
        }

        public void SinkMethodOne(string arg1, string arg2)
        {

        }
    }
}
