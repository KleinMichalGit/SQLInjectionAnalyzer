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
