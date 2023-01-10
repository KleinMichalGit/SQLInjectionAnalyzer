using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.CSProject;
using Model.Method;
using Model.SyntaxTree;
using Model;

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
    }
}
