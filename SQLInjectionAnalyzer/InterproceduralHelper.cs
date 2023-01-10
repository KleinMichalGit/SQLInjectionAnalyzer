using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExceptionService.ExceptionType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model.Method;

namespace SQLInjectionAnalyzer
{
    public class InterproceduralHelper
    {
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
                if (levelBlock.TaintedMethodParameters.Sum() > 0 && levelBlock.NumberOfCallers == 0)
                    return true;

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
                            allMethodInvocations.Add(new InvocationAndParentsTaintedParameters() { InvocationExpression = inv, TaintedMethodParameters = block.TaintedMethodParameters });
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
    }
}
