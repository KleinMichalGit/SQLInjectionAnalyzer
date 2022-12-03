using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Model.Method
{
    /// <summary>
    /// 
    /// </summary>
    public class InvocationAndParentsTaintedParameters
    {
        /// <summary>
        /// The invocation expression
        /// </summary>
        public InvocationExpressionSyntax InvocationExpression;
        /// <summary>
        /// The tainted method parameters
        /// </summary>
        public int[] TaintedMethodParameters;
    }
}
