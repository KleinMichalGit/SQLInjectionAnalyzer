using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Model.Method;
using Model.Rules;

namespace SQLInjectionAnalyzer.Analyzers
{
    public static class RuleEvaluator
    {
        private static readonly TableOfRules TableOfRules = new TableOfRules();
       
        public static void EvaluateRule(
            MethodDeclarationSyntax rootNode,
            SyntaxNode currentNode,
            MethodScanResult result,
            TaintPropagationRules taintPropagationRules,
            List<SyntaxNode> visitedNodes = null,
            int level = 0)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<SyntaxNode>();
            }
            else if (visitedNodes.Contains(currentNode))
            {
                result.AppendEvidence(new string(' ', level * 2) + "HALT (Node already visited)");
                return;
            }

            visitedNodes.Add(currentNode);
            result.AppendEvidence(new string(' ', level * 2) + currentNode);
            level += 1;

            SyntaxNode[] nextLevelNodes = null;

            switch (currentNode)
            {
                case InvocationExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveInvocationExpression(node, result, level, taintPropagationRules);
                    break;
                case ObjectCreationExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveObjectCreationExpression(node);
                    break;
                case AssignmentExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveAssignmentExpression(node);
                    break;
                case VariableDeclaratorSyntax node:
                    nextLevelNodes = TableOfRules.SolveVariableDeclarator(node);
                    break;
                case ConditionalExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveConditionalExpression(node, result, level).Result;
                    break;
                case LiteralExpressionSyntax _:
                    TableOfRules.SolveLiteralExpression(result, level);
                    break;
                case ArgumentSyntax _:
                case IdentifierNameSyntax _:
                    nextLevelNodes = TableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level);
                    break;
                default:
                    TableOfRules.SolveUnrecognizedSyntaxNode(result, currentNode, level);
                    break;
            }

            if (nextLevelNodes == null) return;
            foreach (var t in nextLevelNodes)
            {
                EvaluateRule(rootNode, t, result, taintPropagationRules, visitedNodes, level);
            }
        }
        
        public static void EvaluateRule(
            MethodDeclarationSyntax rootNode,
            SyntaxNode currentNode,
            MethodScanResult result,
            Tainted tainted,
            TaintPropagationRules taintPropagationRules,
            int[] taintedMethodParameters = null,
            List<SyntaxNode> visitedNodes = null,
            int level = 0)
        {
            if (visitedNodes == null)
            {
                visitedNodes = new List<SyntaxNode>();
            }
            else if (visitedNodes.Contains(currentNode))
            {
                result.AppendEvidence(new string(' ', level * 2) + "HALT (Node already visited)");
                return;
            }

            visitedNodes.Add(currentNode);
            result.AppendEvidence(new string(' ', level * 2) + currentNode);
            level += 1;

            SyntaxNode[] nextLevelNodes = null;

            switch (currentNode)
            {
                case InvocationExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveInvocationExpression(node, result, level, taintPropagationRules);
                    break;
                case ObjectCreationExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveObjectCreationExpression(node);
                    break;
                case AssignmentExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveAssignmentExpression(node);
                    break;
                case VariableDeclaratorSyntax node:
                    nextLevelNodes = TableOfRules.SolveVariableDeclarator(node);
                    break;
                case ArgumentSyntax _:
                case IdentifierNameSyntax _:
                    nextLevelNodes = TableOfRules.FindOrigin(rootNode, currentNode, result, visitedNodes, level, tainted);
                    break;
                case ConditionalExpressionSyntax node:
                    nextLevelNodes = TableOfRules.SolveConditionalExpression(node, result, level).Result;
                    break;
                case LiteralExpressionSyntax _:
                    TableOfRules.SolveLiteralExpression(result, level);
                    break;
                default:
                    TableOfRules.SolveUnrecognizedSyntaxNode(result, currentNode, level);
                    break;
            }

            if (nextLevelNodes == null) return;
            for (var i = 0; i < nextLevelNodes.Length; i++)
            {
                // investigate only the origin of arguments which were previously tainted as method parameters
                if (level == 1 && (taintedMethodParameters != null && taintedMethodParameters[i] == 0)) continue;

                var numberOfTaintedMethodParametersBefore = tainted.TaintedMethodParameters.Count(num => num != 0);

                EvaluateRule(rootNode, nextLevelNodes[i], result, tainted, taintPropagationRules, null, visitedNodes, level);

                // for the first invocation only
                // for the parent's tainted method parameters only
                // if following the data flow increased the number of tainted method parameters, it means that current argument of this invocationNode should be tainted
                if (level == 1 && (taintedMethodParameters == null || taintedMethodParameters[i] > 0) && numberOfTaintedMethodParametersBefore < tainted.TaintedMethodParameters.Count(num => num != 0)) tainted.TaintedInvocationArguments[i] += 1;
            }
        }
    }
}