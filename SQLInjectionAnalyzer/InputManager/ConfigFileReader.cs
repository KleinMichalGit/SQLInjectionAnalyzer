using System;
using System.Collections.Generic;
using System.IO;
using ExceptionHandler.ExceptionType;
using Model.Rules;
using Newtonsoft.Json;

namespace SQLInjectionAnalyzer.InputManager
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfigFileReader
    {
        // rules are valid if level > 0, sinkMethods, cleaningMethods, and sourceAreas are defined in config, and all sourceAreas are valid.
        /// <summary>
        /// Ruleses the are valid.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <returns></returns>
        private bool RulesAreValid(TaintPropagationRules rules)
        {
            return rules.Level > 0 && rules.SinkMethods != null && rules.CleaningMethods != null && rules.SourceAreas != null && AllSourceAreasAreValid(rules.SourceAreas);
        }

        // source area is valid if both path and label are defined
        /// <summary>
        /// Alls the source areas are valid.
        /// </summary>
        /// <param name="sourceAreas">The source areas.</param>
        /// <returns></returns>
        private bool AllSourceAreasAreValid(List<SourceArea> sourceAreas)
        {
            foreach (SourceArea sourceArea in sourceAreas)
            {
                if (sourceArea.Path == null || sourceArea.Label == null) return false;
            }
            return true;
        }
        /// <summary>
        /// Processes the configuration.
        /// </summary>
        /// <param name="pathToConfigFile">The path to configuration file.</param>
        /// <returns></returns>
        /// <exception cref="ExceptionHandler.ExceptionType.InvalidInputException">
        /// Can't read the config file.
        /// or
        /// Taint propagation rules are invalid.
        /// </exception>
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
