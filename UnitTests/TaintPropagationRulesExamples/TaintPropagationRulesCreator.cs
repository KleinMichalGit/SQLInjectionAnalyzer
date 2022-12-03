using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Rules;

namespace UnitTests.TaintPropagationRulesExamples
{
    /// <summary>
    /// 
    /// </summary>
    public class TaintPropagationRulesCreator
    {
        /// <summary>
        /// Gets the empty rules.
        /// </summary>
        /// <returns></returns>
        public TaintPropagationRules GetEmptyRules()
        {
            return new TaintPropagationRules();
        }

        /// <summary>
        /// Gets the rules with sink method names.
        /// </summary>
        /// <returns></returns>
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
