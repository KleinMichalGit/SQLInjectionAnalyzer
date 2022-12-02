using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.CodeToBeAnalysed.AssignmentRules.VulnerableAssignment
{
    public class VulnerableAssignment2
    {
        public void ThisIsVulnerableMethod(string arg)
        {
            string vulnerableString1 = arg;
            string vulnerableString2;
            string vulnerableString3;
            string vulnerableString4;

            vulnerableString2 = 1 > 2 ? vulnerableString1 : arg;
            vulnerableString3 = CreateString(vulnerableString2);
            vulnerableString4 = vulnerableString3;

            SinkMethodOne(vulnerableString4);
        }

        public void SinkMethodOne(string arg1)
        {

        }

        private string CreateString(string arg)
        {
            return "";
        }
    }
}
