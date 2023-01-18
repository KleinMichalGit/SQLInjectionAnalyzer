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
                Level = 5,
                SourceAreas = new List<SourceArea>(),
                SinkMethods = new List<string>()
                {
                    "SinkMethodOne",
                    "SinkMethodTwo",
                },
                CleaningMethods = new List<string>()
            };
        }

        public TaintPropagationRules GetRulesWithCleaningMethod()
        {
            return new TaintPropagationRules()
            {
                Level = 5,
                SourceAreas = new List<SourceArea>(),
                SinkMethods = new List<string>(),
                CleaningMethods = new List<string>()
                {
                    "A",
                }
            };
        }
    }
}
