using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public enum ScopeOfAnalysis
    {
        /// <summary>
        /// The simple
        /// </summary>
        Simple,
        /// <summary>
        /// The one method
        /// </summary>
        OneMethod,
        /// <summary>
        /// The interprocedural
        /// </summary>
        Interprocedural,
        /// <summary>
        /// The interprocedural reachability
        /// </summary>
        InterproceduralReachability
    }
}
