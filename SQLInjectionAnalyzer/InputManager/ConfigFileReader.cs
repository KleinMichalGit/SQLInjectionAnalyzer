using System;
using System.Collections.Generic;
using System.IO;
using ExceptionHandler.ExceptionType;
using Model.Rules;
using Newtonsoft.Json;

namespace SQLInjectionAnalyzer.InputManager
{
    /// <summary>
    /// SQLInjectionAnalyzer.InputManager <c>ConfigFileReader</c> class.
    /// 
    /// <para>
    /// Reads and validates .json config files which contain TaintPropagationRules.
    /// 
    /// </para>
    /// <para>
    /// Contains <c>ProcessConfig</c> method.
    /// </para>
    /// </summary>
    public class ConfigFileReader
    {
        /// <summary>
        /// Rules are valid if level > 0, sinkMethods, cleaningMethods, and sourceAreas are defined in config, and all sourceAreas are valid.
        /// </summary>
        private bool RulesAreValid(TaintPropagationRules rules)
        {
            return rules.Level > 0 && rules.SinkMethods != null && rules.CleaningMethods != null && rules.SourceAreas != null && AllSourceAreasAreValid(rules.SourceAreas);
        }

        /// <summary>
        /// Source area is valid if both path and label are defined
        /// </summary>
        private bool AllSourceAreasAreValid(List<SourceArea> sourceAreas)
        {
            foreach (SourceArea sourceArea in sourceAreas)
            {
                if (sourceArea.Path == null || sourceArea.Label == null) return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the .json config on the specified path, converts it from .json format to TaintPropagationRules
        /// format.
        /// </summary>
        /// <param name="pathToConfigFile">The path to .json configuration file.</param>
        /// <returns>If the rules received from the valid .json are valid, returns the rules. Otherwise, throws InvalidInputException.</returns>
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
