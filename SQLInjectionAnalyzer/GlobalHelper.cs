using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>GlobalHelper</c> class.
    /// <para>
    /// Provides global helpful methods. Its methods are used by individual
    /// analyzers.
    /// </para>
    /// <para>
    /// Contains
    /// <c>GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory</c>
    /// method. Contains <c>FindSinkInvocations</c> method.
    /// </para>
    /// </summary>
    public class GlobalHelper
    {
        /// <summary>
        /// Gets the number of files fulfilling certain pattern under the
        /// specified directory. For example returns the number of all C# files
        /// under the specified directory.
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
        /// Finds the sink invocations among the descendant nodes of the
        /// specified syntax node. For example, if method body is the root, it
        /// finds all sink invocations in the body of the method. Invocation is
        /// <see cref="InvocationExpressionSyntax"/> present among the
        /// descendant nodes of the root, which also belongs to the list of sink
        /// methods.
        /// </summary>
        /// <param name="root">The root <see cref="SyntaxNode"/></param>
        /// <param name="sinkMethodNames">The sink method names.</param>
        public IEnumerable<InvocationExpressionSyntax> FindSinkInvocations(SyntaxNode root, List<string> sinkMethodNames)
        {
            IEnumerable<InvocationExpressionSyntax> invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            return invocations.Where(invocation =>
                sinkMethodNames.Any(sinkMethod => invocation.ToString().Contains(sinkMethod))
            );
        }

        public bool MethodShouldBeAnalysed(MethodDeclarationSyntax methodSyntax, SyntaxTreeScanResult syntaxTreeScanResult, TaintPropagationRules rules, bool includePrivate = false)
        {
            if (!includePrivate && !methodSyntax.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }

            if (!methodSyntax.ParameterList.ToString().Contains("string"))
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }

            IEnumerable<InvocationExpressionSyntax> invocations = FindSinkInvocations(methodSyntax, rules.SinkMethods);

            if (!invocations.Any())
            {
                syntaxTreeScanResult.NumberOfSkippedMethods++;
                return false;
            }
            return true;
        }

        public void SolveSourceAreas(SyntaxTree syntaxTree, MethodScanResult methodScanResult, TaintPropagationRules taintPropagationRules)
        {
            foreach (SourceArea source in taintPropagationRules.SourceAreas)
                if (syntaxTree.FilePath.Contains(source.Path))
                    methodScanResult.SourceAreasLabels.Add(source.Label);
        }

        public void WriteEvidenceOnConsole(string methodName, string evidence, MethodScanResult result = null)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Vulnerable method found");
            Console.WriteLine("Method name: " + methodName);
            if (result != null)
            {
                if (result.SourceAreasLabels.Count() > 0)
                {
                    Console.WriteLine("Source areas labels: " + String.Join(", ", result.SourceAreasLabels));
                }
                Console.WriteLine("-----------------------");
                Console.WriteLine("Interprocedural callers tree:");
                PrintCallers(result.InterproceduralCallersTree, 0);
                Console.WriteLine("-----------------------");
            }
            Console.WriteLine("Evidence:");
            Console.WriteLine(evidence);
            Console.WriteLine("-----------------------");
        }
        
        private void PrintCallers(InterproceduralTree callersTree, int level)
        {
            Console.WriteLine(level + " " + new string(' ', level * 2) + callersTree.MethodName);
            
            foreach (var child in callersTree.Callers)
            {
                PrintCallers(child, level + 1);
            }
        }
    }
}