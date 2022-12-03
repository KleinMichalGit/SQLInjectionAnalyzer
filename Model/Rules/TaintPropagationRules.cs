using System.Collections.Generic;

namespace Model.Rules
{
    /// <summary>
    /// 
    /// </summary>
    public class TaintPropagationRules
    {
        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public int Level { get; set; }
        /// <summary>
        /// Gets or sets the source areas.
        /// </summary>
        /// <value>
        /// The source areas.
        /// </value>
        public List<SourceArea> SourceAreas { get; set; }
        /// <summary>
        /// Gets or sets the sink methods.
        /// </summary>
        /// <value>
        /// The sink methods.
        /// </value>
        public List<string> SinkMethods { get; set; }
        /// <summary>
        /// Gets or sets the cleaning methods.
        /// </summary>
        /// <value>
        /// The cleaning methods.
        /// </value>
        public List<string> CleaningMethods { get; set; }
    }
}
