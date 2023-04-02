using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Solution;
using Model.SyntaxTree;

namespace OutputService
{
    /// <summary>
    /// OutputService <c>DataExtractor</c> class.
    /// <para>
    /// Class for extracting, and/or observing data from Diagnostics.
    /// </para>
    /// <para>
    /// Contains <c>DataExtractor</c> constructor. Contains
    /// <c>GetNumberOfAllCSProjFiles</c> method. Contains
    /// <c>GetNumberOfScannedCSProjFiles</c> method. Contains
    /// <c>GetNumberOfSkippedCSProjFiles</c> method. Contains
    /// <c>GetNumberOfAllCSFiles</c> method. Contains
    /// <c>GetNumberOfScannedMethods</c> method. Contains
    /// <c>GetNumberOfSkippedMethods</c> method. Contains
    /// <c>GetNumberOfAllSinks</c> method. Contains
    /// <c>GetNumberOfVulnerableMethods</c> method. Contains
    /// <c>GetNumberOfVulnerableMethodsInFile</c> method. Contains
    /// <c>GetNumberOfVulnerableMethodsInCSProj</c> method.
    /// </para>
    /// </summary>
    public class DataExtractor
    {
        private Diagnostics diagnostics;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExtractor"/> class.
        /// </summary>
        /// <param name="diag">The diagnostics from which the output should be
        ///     created.</param>
        public DataExtractor(Diagnostics diag)
        {
            diagnostics = diag;
        }

        /// <summary>
        /// Gets the number of all csproj files.
        /// </summary>
        public int GetNumberOfAllCSProjFiles()
        {
            int output = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                output += solutionScanResult.NumberOfCSProjFiles;
            }

            return output;
        }

        /// <summary>
        /// Gets the number of all scanned csproj files.
        /// </summary>
        public int GetNumberOfScannedCSProjFiles()
        {
            int output = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                output += solutionScanResult.CSProjectScanResults.Count();
            }
            return output;
        }

        /// <summary>
        /// Gets the number of all skipped csproj files.
        /// </summary>
        public int GetNumberOfSkippedCSProjFiles()
        {
            int output = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                output += solutionScanResult.PathsOfSkippedCSProjects.Count();
            }

            return output;
        }

        /// <summary>
        /// Gets the number of all C# files.
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfAllCSFiles()
        {
            int result = 0;
            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                foreach (CSProjectScanResult scanResult in solutionScanResult.CSProjectScanResults)
                {
                    result += scanResult.NamesOfAllCSFilesInsideThisCSProject.Count();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of scanned methods.
        /// </summary>
        public int GetNumberOfScannedMethods()
        {
            int result = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                foreach (CSProjectScanResult csProjScanResult in solutionScanResult.CSProjectScanResults)
                {
                    foreach (SyntaxTreeScanResult syntaxTreeScanResult in csProjScanResult.SyntaxTreeScanResults)
                    {
                        result += syntaxTreeScanResult.MethodScanResults.Count();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of all skipped methods.
        /// </summary>
        public int GetNumberOfSkippedMethods()
        {
            int result = 0;
            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                foreach (CSProjectScanResult csProjScanResult in solutionScanResult.CSProjectScanResults)
                {
                    foreach (SyntaxTreeScanResult syntaxTreeScanResult in csProjScanResult.SyntaxTreeScanResults)
                    {
                        result += syntaxTreeScanResult.NumberOfSkippedMethods;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of all sinks.
        /// </summary>
        public int GetNumberOfAllSinks()
        {
            int result = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                foreach (CSProjectScanResult csProjScanResult in solutionScanResult.CSProjectScanResults)
                {
                    foreach (SyntaxTreeScanResult syntaxTreeScanResult in csProjScanResult.SyntaxTreeScanResults)
                    {
                        foreach (MethodScanResult methodScanResult in syntaxTreeScanResult.MethodScanResults)
                        {
                            result += methodScanResult.Sinks;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the number of all vulnerable methods.
        /// </summary>
        public int GetNumberOfVulnerableMethods()
        {
            int result = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                foreach (CSProjectScanResult scanResult in solutionScanResult.CSProjectScanResults)
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
            }

            return result;
        }

        /// <summary>
        /// Gets the number of vulnerable methods in one particular file.
        /// </summary>
        /// <param name="syntaxTreeScanResult">The syntax tree scan result from
        ///     which the number of vulnerable methods will be counted.</param>
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

        /// <summary>
        /// Gets the number of vulnerable methods in one particular csproj.
        /// </summary>
        /// <param name="csprojScanResult">The csproj scan result from which the
        ///     number of vulnerable methods will be counted.</param>
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

        public string ShowTree(InterproceduralTree tree)
        {
            return "<ul>" +
                   ShowTreeRec(tree.Callers)
                   + "</ul>";
        }

        private string ShowTreeRec(List<InterproceduralTree> callers)
        {
            string report = "";
            foreach (var caller in callers)
            {
                report += @"<li>
                    <details open>
                    <summary>" + caller.MethodName + "</summary>" + (caller.Callers.Any() ? ShowTree(caller) : "") +
                    @"
                </details>
                    </li>";
            }

            return report;
        }
        
        /// <summary>
        /// Gets the number of vulnerable methods in one particular solution
        /// </summary>
        /// <param name="csprojScanResult">Solution scan result from which the
        ///     number of vulnerable methods will be counted.</param>
        public int GetNumberOfVulnerableMethodsInSolution(SolutionScanResult solutionScanResult)
        {
            int result = 0;

            foreach (CSProjectScanResult csprojScanResult in solutionScanResult.CSProjectScanResults)
            {
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
            }

            return result;
        }

        internal object GetNumberOfAllSolutionFiles()
        {
            return diagnostics.NumberOfSolutions;
        }

        internal object GetNumberOfScannedSolutionFiles()
        {
            int result = 0;

            foreach (SolutionScanResult solutionScanResult in diagnostics.SolutionScanResults)
            {
                result += solutionScanResult.CSProjectScanResults.Count();
            }

            return result;
        }

        internal object GetNumberOfSkippedSolutionFiles()
        {
            return diagnostics.PathsOfSkippedSolutions.Count();
        }
    }
}