using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Model.Method
{
    public class InvocationAndParentsTaintedParameters
    {
        public InvocationExpressionSyntax InvocationExpression;
        public int[] TaintedMethodParameters;
    }
}
