using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.CleaningRules.AllArgumentsAreCleaned
{
    public class ArgumentsAreCleaned2
    {
        public void ThisIsSafeMethod(string args)
        {
            SinkMethodOne("this is const");
        }

        public void SinkMethodOne(string args)
        {

        }
    }
}
