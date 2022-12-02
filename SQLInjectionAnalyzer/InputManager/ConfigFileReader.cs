using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExceptionHandler.ExceptionType;
using Model.Rules;
using Newtonsoft.Json;

namespace SQLInjectionAnalyzer.InputManager
{
    public class ConfigFileReader
    {
        // rules are valid if level > 0, sinkMethods, cleaningMethods, and sourceAreas are defined in config, and all sourceAreas are valid.
        private bool RulesAreValid(TaintPropagationRules rules)
        {
            return rules.Level > 0 && rules.SinkMethods != null && rules.CleaningMethods != null && rules.SourceAreas != null && AllSourceAreasAreValid(rules.SourceAreas);
        }

        // source area is valid if both path and label are defined
        private bool AllSourceAreasAreValid(List<SourceArea> sourceAreas)
        {
            foreach (SourceArea sourceArea in sourceAreas)
            {
                if (sourceArea.Path == null || sourceArea.Label == null) return false;
            }
            return true;
        }
        public TaintPropagationRules ProcessConfig(string pathToConfigFile)
        {
            try
            {
                using (StreamReader r = new StreamReader(pathToConfigFile))
                {
                    string json = r.ReadToEnd();
                    TaintPropagationRules rules = JsonConvert.DeserializeObject<TaintPropagationRules>(json);
                    if (RulesAreValid(rules)) return rules;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                throw new InvalidInputException("Can't read the config file.");
            }


            throw new InvalidInputException("Taint propagation rules are invalid.");
        }
    }
}
