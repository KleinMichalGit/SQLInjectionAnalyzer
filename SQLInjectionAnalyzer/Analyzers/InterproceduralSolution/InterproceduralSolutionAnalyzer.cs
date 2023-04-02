using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Model;
using Model.CSProject;
using Model.Method;
using Model.Rules;
using Model.Solution;
using Model.SyntaxTree;

namespace SQLInjectionAnalyzer.Analyzers.InterproceduralSolution
{
    public class InterproceduralSolutionAnalyzer : Analyzer
    {
        private TaintPropagationRules taintPropagationRules;
        private const string TargetFileType = "*.sln";
        private CSProjectScanResult csprojScanResult = new CSProjectScanResult();
        private bool writeOnConsole;
        private readonly GlobalHelper globalHelper = new GlobalHelper();
        private readonly DiagnosticsInitializer diagnosticsInitializer = new DiagnosticsInitializer();
        private readonly InterproceduralHelper interproceduralHelper = new InterproceduralHelper();
        private List<string> excludeSubpaths = new List<string>();

        public override Diagnostics ScanDirectory(string directoryPath, List<string> excludeSubpaths, TaintPropagationRules taintPropagationRules, bool writeOnConsole)
        {
            this.excludeSubpaths = excludeSubpaths;
            this.taintPropagationRules = taintPropagationRules;
            this.writeOnConsole = writeOnConsole;
            var diagnostics = diagnosticsInitializer.InitialiseDiagnostics(ScopeOfAnalysis.InterproceduralSolution);

            var numberOfSolutionFilesUnderThisRepository = globalHelper.GetNumberOfFilesFulfillingCertainPatternUnderThisDirectory(directoryPath, TargetFileType);
            var numberOfScannedSolutionFilesSoFar = 0;
            diagnostics.NumberOfSolutions = numberOfSolutionFilesUnderThisRepository;

            foreach (string solutionFilePath in Directory.EnumerateFiles(directoryPath, TargetFileType, SearchOption.AllDirectories))
            {
                SolutionScanResult solutionScanResult = diagnosticsInitializer.InitializeSolutionScanResult(solutionFilePath);

                // skip all blacklisted .sln files
                if (excludeSubpaths.Any(x => solutionFilePath.Contains(x)))
                {
                    diagnostics.PathsOfSkippedSolutions.Add(solutionFilePath);
                }
                else
                {
                    Console.WriteLine("currently scanned .sln: " + solutionFilePath);
                    Console.WriteLine(numberOfScannedSolutionFilesSoFar + " / " + numberOfSolutionFilesUnderThisRepository + " .sln files scanned");
                    ScanSolution(solutionFilePath, solutionScanResult).Wait();
                    solutionScanResult.CSProjectScanResults.Add(csprojScanResult);
                }

                solutionScanResult.SolutionScanResultEndTime = DateTime.Now;

                diagnostics.SolutionScanResults.Add(solutionScanResult);
                numberOfScannedSolutionFilesSoFar++;
            }

            Console.WriteLine(numberOfScannedSolutionFilesSoFar + " / " + numberOfSolutionFilesUnderThisRepository + " .sln files scanned");

            diagnostics.DiagnosticsEndTime = DateTime.Now;

            return diagnostics;
        }

        private async Task ScanSolution(string solutionPath, SolutionScanResult solutionScanResult)
        {
            using (var workspace = MSBuildWorkspace.Create())
            {
                var solution = await workspace.OpenSolutionAsync(solutionPath);
                foreach (var project in solution.Projects)
                {
                    solutionScanResult.NumberOfCSProjFiles++;
                    if (excludeSubpaths.Any(x => project.FilePath != null && project.FilePath.Contains(x)))
                    {
                        solutionScanResult.PathsOfSkippedCSProjects.Add(project.FilePath);
                    }
                    else
                    {
                        Console.WriteLine("    + project: " + project.FilePath);
                        ScanCsProj(project, solution).Wait();
                    }
                }
            }
        }

        private async Task ScanCsProj(Project project, Solution solution)
        {
            csprojScanResult = diagnosticsInitializer.InitialiseCSProjectScanResult(project.FilePath);

            if (project.FilePath != null && project.FilePath.EndsWith(".vbproj"))
            {
                csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
                return;
            }

            var compilation = await project.GetCompilationAsync();

            if (compilation != null)
                foreach (var st in compilation.SyntaxTrees)
                {
                    var syntaxTree = (CSharpSyntaxTree)st;
                    csprojScanResult.NamesOfAllCSFilesInsideThisCSProject.Add(syntaxTree.FilePath);
                    var syntaxTreeScanResult = ScanSyntaxTree(syntaxTree, solution, compilation);
                    csprojScanResult.SyntaxTreeScanResults.Add(syntaxTreeScanResult);
                }

            csprojScanResult.CSProjectScanResultEndTime = DateTime.Now;
        }

        private SyntaxTreeScanResult ScanSyntaxTree(CSharpSyntaxTree syntaxTree, Solution solution, Compilation compilation)
        {
            var syntaxTreeScanResult = diagnosticsInitializer.InitialiseSyntaxTreeScanResult(syntaxTree.FilePath);

            foreach (var methodSyntax in syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!globalHelper.MethodShouldBeAnalysed(methodSyntax, syntaxTreeScanResult, taintPropagationRules, true)) continue;

                var methodScanResult = InterproceduralSolutionScanMethod(methodSyntax, solution, compilation);
                
                // these values are not set for method scans without hits,
                // because it resulted into OutOfMemoryException when analysing monorepository
                if (methodScanResult.Hits > 0)
                {
                    methodScanResult.MethodName = methodSyntax.Identifier.ToString() + methodSyntax.ParameterList;
                    methodScanResult.MethodBody = methodSyntax.ToString();
                    var lineSpan = methodSyntax.GetLocation().GetLineSpan();
                    methodScanResult.LineNumber = lineSpan.StartLinePosition.Line;
                    methodScanResult.LineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line;

                    if (writeOnConsole)
                    {
                        globalHelper.WriteEvidenceOnConsole(methodScanResult.MethodName, methodScanResult.Evidence, methodScanResult);
                    }
                }

                if (methodScanResult.Hits == 0) // if all tainted variables are cleaned, we do not need to remember anything
                {
                    methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();
                }

                syntaxTreeScanResult.MethodScanResults.Add(methodScanResult);
            }
            syntaxTreeScanResult.SyntaxTreeScanResultEndTime = DateTime.Now;
            return syntaxTreeScanResult;
        }

        private MethodScanResult InterproceduralSolutionScanMethod(MethodDeclarationSyntax methodSyntax, Solution solution, Compilation compilation)
        {
            var methodScanResult = ConductScanOfTheInitialMethod(methodSyntax);
            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree, ignoreAccessibility: false);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodSyntax);

            var random = new Random();
            var tree = new InterproceduralTree
            {
                Id = random.Next(),
                MethodName = methodSymbol?.ToString(),
                Callers = new List<InterproceduralTree>()
            };
            
            var currentLevelBlocks = new List<LevelBlock>() { new LevelBlock() { MethodSymbol = methodSymbol, TaintedMethodParameters = methodScanResult.TaintedMethodParameters, InterproceduralCallersTreeNodeId = tree.Id} };
            var nextLevelBlocks = new List<LevelBlock>();

            // n-level BFS solution interprocedural analysis
            for (var currentLevel = 2; currentLevel < taintPropagationRules.Level + 1; currentLevel++)
            {
                var allMethodInvocations = interproceduralHelper.FindAllCallersOfCurrentBlockInSolutionAsync(currentLevelBlocks, methodScanResult, solution, taintPropagationRules);

                foreach (var invocation in allMethodInvocations)
                {
                    semanticModel = invocation.Compilation.GetSemanticModel(invocation.InvocationExpression.SyntaxTree, ignoreAccessibility: false);

                    var parent = interproceduralHelper.FindMethodParent(invocation.InvocationExpression.Parent);

                    if (parent == null) continue;

                    var id = random.Next();
                    
                    interproceduralHelper.AddCaller(
                        invocation.InterproceduralCallersTreeCalleeNodeId,
                        tree, 
                        new InterproceduralTree
                        {
                            Id = id,
                            MethodName = semanticModel.GetDeclaredSymbol(parent)?.ToString(),
                            Callers = new List<InterproceduralTree>()
                        }
                    );
                    
                    methodScanResult.AppendEvidence("INTERPROCEDURAL LEVEL: " + currentLevel + " " + semanticModel.GetDeclaredSymbol(parent));

                    var tainted = new Tainted()
                    {
                        TaintedMethodParameters = new int[parent.ParameterList.Parameters.Count()],
                        TaintedInvocationArguments = new int[invocation.InvocationExpression.ArgumentList.Arguments.Count()]
                    };

                    RuleEvaluator.EvaluateRule(parent, invocation.InvocationExpression, methodScanResult, tainted, taintPropagationRules, invocation.TaintedMethodParameters);

                    if (interproceduralHelper.AllTaintVariablesAreCleanedInThisBranch(invocation.TaintedMethodParameters, tainted.TaintedInvocationArguments))
                    {
                        methodScanResult.AppendEvidence("ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.");
                    }
                    else
                    {
                        nextLevelBlocks.Add(new LevelBlock() { TaintedMethodParameters = tainted.TaintedMethodParameters, MethodSymbol = semanticModel.GetDeclaredSymbol(parent), InterproceduralCallersTreeNodeId = id});
                    }
                }

                // on current level we have at least one method with tainted parameters, but this method has 0 callers. Therefore its parameters
                // will never be cleaned.
                if (interproceduralHelper.CurrentLevelContainsTaintedBlocksWithoutCallers(currentLevelBlocks))
                {
                    methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, THERE IS AT LEAST ONE METHOD WITH TAINTED PARAMETERS WITHOUT ANY CALLERS. THEREFORE, ITS PARAMETERS ARE UNCLEANABLE.");
                    methodScanResult.InterproceduralCallersTree = tree;
                    methodScanResult.MethodScanResultEndTime = DateTime.Now;
                    return methodScanResult;
                }

                currentLevelBlocks = nextLevelBlocks;
                nextLevelBlocks = new List<LevelBlock>();

                //all tainted parameters are cleaned
                if (currentLevelBlocks.Any()) continue;
                methodScanResult.AppendEvidence("ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, ALL TAINTED VARIABLES WERE CLEANED.");
                methodScanResult.InterproceduralCallersTree = tree;
                methodScanResult.MethodScanResultEndTime = DateTime.Now;
                return methodScanResult;
            }

            
            methodScanResult.InterproceduralCallersTree = tree;
            
            methodScanResult.MethodScanResultEndTime = DateTime.Now;
            return methodScanResult;
        }

        private MethodScanResult ConductScanOfTheInitialMethod(MethodDeclarationSyntax methodSyntax)
        {
            var methodScanResult = diagnosticsInitializer.InitialiseMethodScanResult();
            methodScanResult.AppendEvidence("INTERPROCEDURAL LEVEL: 1 ");

            var invocations = globalHelper.FindSinkInvocations(methodSyntax, taintPropagationRules.SinkMethods).ToList();
            methodScanResult.Sinks = (short)invocations.Count();

            var taintedMethod = new Tainted()
            {
                TaintedMethodParameters = new int[methodSyntax.ParameterList.Parameters.Count()],
            };

            // follows data flow inside method for each sink invocation from sink invocation to source
            foreach (var invocation in invocations)
            {
                var taintedInvocation = new Tainted()
                {
                    TaintedMethodParameters = new int[methodSyntax.ParameterList.Parameters.Count()],
                    TaintedInvocationArguments = new int[invocation.ArgumentList.Arguments.Count()]
                };

                RuleEvaluator.EvaluateRule(methodSyntax, invocation, methodScanResult, taintedInvocation, taintPropagationRules);

                for (var i = 0; i < taintedMethod.TaintedMethodParameters.Length; i++)
                {
                    taintedMethod.TaintedMethodParameters[i] += taintedInvocation.TaintedMethodParameters[i];
                }
            }

            methodScanResult.TaintedMethodParameters = taintedMethod.TaintedMethodParameters;
            return methodScanResult;
        }
    }
}