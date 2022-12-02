using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.InvocationRules
{
    public class InvocationTestClass2
    {
        public void ThisIsSaveMethod(string arg)
        {
            const string stringConst = "thisIsConst";

            string arg1 = CreateStringValue(stringConst);
            string arg2 = CreateStringValue(string.Empty);

            string arg3 = CreateStringValue(arg1);
            string arg4 = CreateStringValue(arg2);

            SinkMethodOne(arg3, arg4);
        }

        private string CreateStringValue(string arg)
        {
            return arg;
        }

        private void SinkMethodOne(string arg1, string arg2)
        {

        }
    }
}
