using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.SyntaxTree;

namespace SQLInjectionAnalyzer
{
    /// <summary>
    /// SQLInjectionAnalyzer <c>GlobalHelper</c> class.
    /// 
    /// <para>
    /// Provides global helpful methods. Its methods are used by individual analyzers.
    /// 
    /// </para>
    /// <para>
    /// Contains <c>GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory</c> method.
    /// Contains <c>FindSinkInvocations</c> method.
    /// </para>
    /// </summary>
    public class GlobalHelper
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
            IEnumerable<InvocationExpressionSyntax> invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            return invocations.Where(invocation =>
                sinkMethodNames.Any(sinkMethod => invocation.ToString().Contains(sinkMethod))
            );
        }

        public Diagnostics InitialiseDiagnostics(ScopeOfAnalysis scopeOfAnalysis)
        {
            Diagnostics diagnostics = new Diagnostics();
            diagnostics.ScopeOfAnalysis = scopeOfAnalysis;
            diagnostics.DiagnosticsStartTime = DateTime.Now;
            return diagnostics;
        }


        public CSProjectScanResult InitialiseScanResult(string directoryPath)
        {
            CSProjectScanResult scanResult = new CSProjectScanResult();
            scanResult.CSProjectScanResultStartTime = DateTime.Now;
            scanResult.Path = directoryPath;

            return scanResult;
        }

        public SyntaxTreeScanResult InitialiseSyntaxTreeScanResult(string filePath)
        {
            SyntaxTreeScanResult syntaxTreeScanResult = new SyntaxTreeScanResult();
            syntaxTreeScanResult.SyntaxTreeScanResultStartTime = DateTime.Now;
            syntaxTreeScanResult.Path = filePath;

            return syntaxTreeScanResult;
        }

        public MethodScanResult InitialiseMethodScanResult()
        {
            MethodScanResult methodScanResult = new MethodScanResult();
            methodScanResult.MethodScanResultStartTime = DateTime.Now;

            return methodScanResult;
        }

        public bool MethodShouldBeAnalysed(MethodDeclarationSyntax methodSyntax, SyntaxTreeScanResult syntaxTreeScanResult, TaintPropagationRules rules)
        {
            if (!methodSyntax.Modifiers.Where(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)).Any())
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


        public void WriteEvidenceOnConsole(string methodName, string evidence)
        {
            Console.WriteLine("-----------------------");
            Console.WriteLine("Vulnerable method found");
            Console.WriteLine("Method name: " + methodName);
            Console.WriteLine("Evidence:");
            Console.WriteLine(evidence);
            Console.WriteLine("-----------------------");
        }
    }
}
