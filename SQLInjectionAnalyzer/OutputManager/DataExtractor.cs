using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.CSProject;
using Model.Method;
using Model.SyntaxTree;
using Model;

namespace SQLInjectionAnalyzer.OutputManager
{
    public class DataExtractor
    {

        private Diagnostics diagnostics;

        public DataExtractor(Diagnostics diag)
        {
            diagnostics = diag;
        }

        public int GetNumberOfAllCSProjFiles()
        {
            return diagnostics.NumberOfCSProjFiles;
        }

        public int GetNumberOfScannedCSProjFiles()
        {
            return diagnostics.CSProjectScanResults.Count();
        }

        public int GetNumberOfSkippedCSProjFiles()
        {
            return diagnostics.PathsOfSkippedCSProjects.Count();
        }

        public int GetNumberOfAllCSFiles()
        {
            int result = 0;

            foreach (CSProjectScanResult scanResult in diagnostics.CSProjectScanResults)
            {
                result += scanResult.NamesOfAllCSFilesInsideThisCSProject.Count();
            }

            return result;
        }

        public int GetNumberOfScannedMethods()
        {
            int result = 0;

            foreach (CSProjectScanResult scanResult in diagnostics.CSProjectScanResults)
            {
                foreach (SyntaxTreeScanResult syntaxTreeScanResult in scanResult.SyntaxTreeScanResults)
                {
                    result += syntaxTreeScanResult.MethodScanResults.Count();
                }
            }

            return result;
        }

        public int GetNumberOfSkippedMethods()
        {
            int result = 0;

            foreach (CSProjectScanResult scanResult in diagnostics.CSProjectScanResults)
            {
                foreach (SyntaxTreeScanResult syntaxTreeScanResult in scanResult.SyntaxTreeScanResults)
                {
                    result += syntaxTreeScanResult.NumberOfSkippedMethods;
                }
            }

            return result;
        }

        public int GetNumberOfAllSinks()
        {
            int result = 0;

            foreach (CSProjectScanResult scanResult in diagnostics.CSProjectScanResults)
            {
                foreach (SyntaxTreeScanResult syntaxTreeScanResult in scanResult.SyntaxTreeScanResults)
                {
                    foreach (MethodScanResult methodScanResult in syntaxTreeScanResult.MethodScanResults)
                    {
                        result += methodScanResult.Sinks;
                    }
                }
            }

            return result;
        }

        public int GetNumberOfVulnerableMethods()
        {
            int result = 0;

            foreach (CSProjectScanResult scanResult in diagnostics.CSProjectScanResults)
            {
                foreach (SyntaxTreeScanResult syntaxTreeScanResult in scanResult.SyntaxTreeScanResults)
                {
                    foreach (MethodScanResult methodScanResult in syntaxTreeScanResult.MethodScanResults)
                    {
                        if (methodScanResult.Hits > 0)
                        {
                            result++;
                        }
                    }
                }
            }

            return result;
        }

        public int GetNumberOfVulnerableMethodsInFile(SyntaxTreeScanResult syntaxTreeScanResult)
        {
            int result = 0;

            foreach (MethodScanResult methodScanResult in syntaxTreeScanResult.MethodScanResults)
            {
                if (methodScanResult.Hits > 0)
                {
                    result++;
                }
            }

            return result;
        }

        public int GetNumberOfVulnerableMethodsInCSProj(CSProjectScanResult csprojScanResult)
        {
            int result = 0;

            foreach (SyntaxTreeScanResult syntaxTreeScanResult in csprojScanResult.SyntaxTreeScanResults)
            {
                foreach (MethodScanResult methodScanResult in syntaxTreeScanResult.MethodScanResults)
                {
                    if (methodScanResult.Hits > 0)
                    {
                        result++;
                    }
                }
            }

            return result;
        }
    }
}
