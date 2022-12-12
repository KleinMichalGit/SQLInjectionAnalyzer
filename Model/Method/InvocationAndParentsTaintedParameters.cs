using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Model.Method
{

    public class InvocationAndParentsTaintedParameters
    {
   
        public InvocationExpressionSyntax InvocationExpression;
        
        public int[] TaintedMethodParameters;
    }
}
