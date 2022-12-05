using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>CommonSyntaxHelper</c> class.
    /// 
    /// <para>
    /// Provides helpful methods for working with syntax. Its methods are used by individual analyzers.
    /// 
    /// </para>
    /// <para>
    /// Contains <c>GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory</c> method.
    /// Contains <c>FindSinkInvocations</c> method.
    /// </para>
    /// </summary>
    public class CommonSyntaxHelper
    {
        /// <summary>
        /// Gets the number of files fulfilling certain pattern under the specified directory.
        /// For example returns the number of all C# files under the specified directory.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="pattern">The pattern.</param>
        public int GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(string directoryPath, string pattern)
        {
            int result = 0;
            foreach (string _ in Directory.EnumerateFiles(directoryPath, pattern, SearchOption.AllDirectories)) result++;
            return result;
        }

        /// <summary>
        /// Finds the sink invocations among the descendant nodes of the specified syntax node. For example,
        /// if method body is the root, it finds all sink invocations in the body of the method. Invocation is
        /// <see cref="InvocationExpressionSyntax"/> present among the descendant nodes of the root, which also 
        /// belongs to the list of sink methods.
        /// 
        /// </summary>
        /// <param name="root">The root <see cref="SyntaxNode"/> </param>
        /// <param name="sinkMethodNames">The sink method names.</param>
        public IEnumerable<InvocationExpressionSyntax> FindSinkInvocations(SyntaxNode root, List<string> sinkMethodNames)
        {
            // TODO: replace simple string comparison with more robust approach 
            IEnumerable<InvocationExpressionSyntax> invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            return invocations.Where(invocation =>
                sinkMethodNames.Any(sinkMethod => invocation.ToString().Contains(sinkMethod))
            );
        }
    }
}
