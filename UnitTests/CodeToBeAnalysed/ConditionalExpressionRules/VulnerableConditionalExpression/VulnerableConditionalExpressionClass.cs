using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.ConditionalExpressionRules.VulnerableConditionalExpression
{
    public class VulnerableConditionalExpressionClass
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            string thisIsConditionalExpression = 1 < 2 ? "the universe works" : arg;

            SinkMethodOne(thisIsConditionalExpression);
        }

        public void SinkMethodOne(string arg1)
        {

        }
    }
}
