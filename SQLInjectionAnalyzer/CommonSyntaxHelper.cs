using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SQLInjectionAnalyzer
{
    public class CommonSyntaxHelper
    {
        public int GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(string directoryPath, string pattern)
        {
            int result = 0;
            foreach (string _ in Directory.EnumerateFiles(directoryPath, pattern, SearchOption.AllDirectories)) result++;
            return result;
        }

        public IEnumerable<InvocationExpressionSyntax> FindSinkInvocations(SyntaxNode root, List<string> sinkMethodNames)
        {
            // TODO: replace simple string compare with more robust approach 
            IEnumerable<InvocationExpressionSyntax> invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            return invocations.Where(invocation =>
                sinkMethodNames.Any(sinkMethod => invocation.ToString().Contains(sinkMethod))
            );
        }
    }
}
