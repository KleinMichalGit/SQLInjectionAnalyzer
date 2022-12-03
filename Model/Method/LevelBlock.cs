﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Model.Method
{
    /// <summary>
    /// 
    /// </summary>
    public class LevelBlock
    {
        /// <summary>
        /// The method symbol
        /// </summary>
        public IMethodSymbol MethodSymbol;
        /// <summary>
        /// The tainted method parameters
        /// </summary>
        public int[] TaintedMethodParameters;
        /// <summary>
        /// The number of callers
        /// </summary>
        public int NumberOfCallers;
    }
}
