using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.ConditionalExpressionRules.SafeConditionalExpression
{
    public class SafeConditionalExpressionClass
    {
        public void ThisIsSafeMethod(string arg)
        {
            const string thisIsConditionalExpression = 1 < 2 ? "the universe works" : "the universe does not work";

            SinkMethodOne(thisIsConditionalExpression);
        }

        public void SinkMethodOne(string arg1)
        {

        }
    }
}
