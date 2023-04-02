using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExceptionService.ExceptionType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model.Method;
using Model.Rules;

namespace SQLInjectionAnalyzer
{
    public class InterproceduralHelper
    {
        private Compilation compilation = null;

        public MethodDeclarationSyntax FindMethodParent(SyntaxNode parent)
        {
            while (parent != null)
            {
                if (parent is MethodDeclarationSyntax)
                {
                    return (MethodDeclarationSyntax)parent;
                }
                parent = parent.Parent;
            }
            return null;
        }

        public bool CurrentLevelContainsTaintedBlocksWithoutCallers(List<LevelBlock> currentLevelBlocks)
        {
            foreach (LevelBlock levelBlock in currentLevelBlocks)
            {
                if (levelBlock.TaintedMethodParameters.Sum() > 0 && levelBlock.NumberOfCallers == 0)
                    return true;
            }

            return false;
        }

        public bool AllTaintVariablesAreCleanedInThisBranch(int[] parentMethodTainted, int[] invocationTainted)
        {
            if (parentMethodTainted.Length != invocationTainted.Length) throw new AnalysisException("number of tainted method parameters and invocation arguments is incorrect!");

            for (int i = 0; i < parentMethodTainted.Length; i++)
            {
                if (parentMethodTainted[i] > 0 && invocationTainted[i] > 0)
                    return false;
            }
            return true;
        }

        public List<InvocationAndParentsTaintedParameters> FindAllCallersOfCurrentBlock(SyntaxTree currentSyntaxTree, List<LevelBlock> currentLevelBlocks, SemanticModel semanticModel, MethodScanResult methodScanResult)
        {
            IEnumerable<InvocationExpressionSyntax> allInvocations = currentSyntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();
            List<InvocationAndParentsTaintedParameters> allMethodInvocations = new List<InvocationAndParentsTaintedParameters>();

            // find all invocations with same symbol info AND number of parameters
            foreach (LevelBlock block in currentLevelBlocks) // method
            {
                foreach (InvocationExpressionSyntax inv in allInvocations) //it's invocation
                {
                    if (block.MethodSymbol == semanticModel.GetSymbolInfo(inv).Symbol)
                    {
                        if (block.MethodSymbol.Parameters.Count() == inv.ArgumentList.Arguments.Count())
                        {
                            block.NumberOfCallers += 1;
                            allMethodInvocations.Add(new InvocationAndParentsTaintedParameters() { InvocationExpression = inv, TaintedMethodParameters = block.TaintedMethodParameters, InterproceduralCallersTreeCalleeNodeId = block.InterproceduralCallersTreeNodeId});
                        }
                        else
                        {
                            methodScanResult.AppendEvidence("THERE IS A CALLER OF METHOD " + block.MethodSymbol.ToString() + " BUT WITH A DIFFERENT AMOUNT OF ARGUMENTS (UNABLE TO DECIDE WHICH TAINTED ARGUMENT IS WHICH)");
                        }
                    }
                }
            }
            return allMethodInvocations;
        }

        public List<InvocationAndParentsTaintedParameters> FindAllCallersOfCurrentBlockInSolutionAsync(List<LevelBlock> currentLevelBlocks, MethodScanResult methodScanResult, Solution solution, TaintPropagationRules taintPropagationRules)
        {
            List<InvocationAndParentsTaintedParameters> allMethodInvocations = new List<InvocationAndParentsTaintedParameters>();
            foreach (Project project in solution.Projects)
            {
                SetCompilation(project).Wait();
                Compilation compilation = this.compilation;

                foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
                {
                    new GlobalHelper().SolveSourceAreas(syntaxTree, methodScanResult, taintPropagationRules);

                    IEnumerable<InvocationExpressionSyntax> allInvocations = syntaxTree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();

                    // find all invocations with same symbol info AND number of parameters
                    foreach (LevelBlock block in currentLevelBlocks) // method
                    {
                        foreach (InvocationExpressionSyntax inv in allInvocations) //it's invocation
                        {
                            if (block.MethodSymbol.Name == ComputeName(inv.ToString()))
                            {
                                if (block.MethodSymbol.Parameters.Count() == inv.ArgumentList.Arguments.Count())
                                {
                                    block.NumberOfCallers += 1;
                                    allMethodInvocations.Add(new InvocationAndParentsTaintedParameters() { InvocationExpression = inv, TaintedMethodParameters = block.TaintedMethodParameters, Compilation = compilation, InterproceduralCallersTreeCalleeNodeId = block.InterproceduralCallersTreeNodeId});
                                }
                                else
                                {
                                    methodScanResult.AppendEvidence("THERE IS A CALLER OF METHOD " + block.MethodSymbol.ToString() + " BUT WITH A DIFFERENT AMOUNT OF ARGUMENTS (UNABLE TO DECIDE WHICH TAINTED ARGUMENT IS WHICH)");
                                }
                            }
                        }
                    }
                }
            }

            return allMethodInvocations;
        }

        public void AddCaller(int id, InterproceduralTree callersTree, InterproceduralTree caller)
        {
            if (callersTree.Id == id)
            {
                callersTree.Callers.Add(caller);
                return;
            }
            
            foreach (var child in callersTree.Callers)
            {
                AddCaller(id, child, caller);
            }
        }

        private async Task SetCompilation(Project project)
        {
            this.compilation = await project.GetCompilationAsync();
        }

        private string ComputeName(string fullName)
        {
            string name = fullName.Substring(0, fullName.IndexOf("(")); // take until first (

            if (name.Contains("."))
            {
                name = name.Substring(name.LastIndexOf(".") + 1, name.Length - 1 - (name.LastIndexOf(".")));
            }
            return name;
        }
    }
}