using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Rules;

namespace UnitTests.TaintPropagationRulesExamples
{
    public class TaintPropagationRulesCreator
    {
        public TaintPropagationRules GetEmptyRules()
        {
            return new TaintPropagationRules();
        }

        public TaintPropagationRules GetRulesWithSinkMethodNames()
        {
            return new TaintPropagationRules()
            {
                SinkMethods = new List<string>()
                {
                    "SinkMethodOne",
                    "SinkMethodTwo",
                },
                CleaningMethods = new List<string>()
            };
        }
    }
}
