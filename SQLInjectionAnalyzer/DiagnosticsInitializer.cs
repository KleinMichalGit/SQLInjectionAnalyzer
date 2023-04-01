using System;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Solution;
using Model.SyntaxTree;

namespace SQLInjectionAnalyzer
{
    public class DiagnosticsInitializer
    {
        public Diagnostics InitialiseDiagnostics(ScopeOfAnalysis scopeOfAnalysis)
        {
            Diagnostics diagnostics = new Diagnostics();
            diagnostics.ScopeOfAnalysis = scopeOfAnalysis;
            diagnostics.DiagnosticsStartTime = DateTime.Now;
            return diagnostics;
        }

        public SolutionScanResult InitializeSolutionScanResult(string solutionPath)
        {
            SolutionScanResult solutionScanResult = new SolutionScanResult();
            solutionScanResult.SolutionScanResultStartTime = DateTime.Now;
            solutionScanResult.Path = solutionPath;

            return solutionScanResult;
        }

        public CSProjectScanResult InitialiseCSProjectScanResult(string directoryPath)
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
    }
}