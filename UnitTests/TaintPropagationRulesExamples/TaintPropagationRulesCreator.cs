using System.Collections.Generic;
using Model.Rules;

namespace UnitTests.TaintPropagationRulesExamples
{
    /// <summary>
    /// common unit test helper for creating custom Taint variable propagation
    /// rules <see cref="TaintPropagationRules"/>.
    /// </summary>
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
