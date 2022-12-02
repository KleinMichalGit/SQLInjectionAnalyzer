using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Model.Method
{
    public class LevelBlock
    {
        public IMethodSymbol MethodSymbol;
        public int[] TaintedMethodParameters;
        public int NumberOfCallers;
    }
}
