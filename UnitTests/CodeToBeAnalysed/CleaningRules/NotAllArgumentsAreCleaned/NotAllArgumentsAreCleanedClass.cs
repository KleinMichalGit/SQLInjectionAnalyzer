using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.CleaningRules.NotAllArgumentsAreCleaned
{
    public class NotAllArgumentsCleanedClass
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
