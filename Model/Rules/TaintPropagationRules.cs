using System.Collections.Generic;

namespace Model.Rules
{
    /// <summary>
    /// Model.Rules <c>TaintPropagationRules</c> class.
    /// <para>
    /// Contains all the information gained from the .json config file for
    /// solving the taint variable propagation problems. Each taint variable
    /// propagation problem will be solved with the consideration of these
    /// rules, therefore by changing these rules it is possible to manipulate
    /// with the final result and adjust it according to our needs.
    /// </para>
    /// </summary>
    public class TaintPropagationRules
    {
        /// <summary>
        /// Maximal level of recursion in method-body blocks during
        /// interprocedural analysis. If the Scope is not interprocedural, then
        /// the Level will not be taken into consideration during analysis.
        /// </summary>
        /// <value>integer - max level of recursion during interp. analysis.
        ///     </value>
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets the source areas used to label the results for easier
        /// orientation in results.
        /// </summary>
        /// <value>The list of source areas.</value>
        public List<SourceArea> SourceAreas { get; set; }

        /// <summary>
        /// Gets or sets the list of sink methods. Sink methods are methods
        /// which are considered as potentialy vulnerable if at least one
        /// unchecked user-provided argument is passed to them. For example
        /// methods which communicate with a database.
        /// </summary>
        /// <value>The list of sink methods.</value>
        public List<string> SinkMethods { get; set; }

        /// <summary>
        /// Gets or sets the list of cleaning methods. Cleaning methods are
        /// methods which are considered as safe under any circumstances, and at
        /// any time, any argument can be passed to them without the need to
        /// track the argument. It is immediately considered as cleaned and
        /// tracking the argument may be considered as finished.
        /// </summary>
        /// <value>The list of cleaning methods.</value>
        public List<string> CleaningMethods { get; set; }
    }
}