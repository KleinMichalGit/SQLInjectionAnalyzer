﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Model.Method;
using Model.Rules;

namespace SQLInjectionAnalyzer.Analyzers
{
    public class TableOfRules
    {
        public SyntaxNode[] SolveInvocationExpression(InvocationExpressionSyntax invocationNode, MethodScanResult result, int level, TaintPropagationRules taintPropagationRules)
        {
            if (!taintPropagationRules.CleaningMethods.Any(cleaningMethod =>
                    invocationNode.ToString().Contains(cleaningMethod)))
                return invocationNode.ArgumentList.Arguments.ToArray();
            result.AppendEvidence(new string(' ', level * 2) + "OK (Cleaning method)");
            return null;
        }

        public SyntaxNode[] SolveObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationNode)
        {
            return objectCreationNode.ArgumentList?.Arguments.ToArray();
        }

        public SyntaxNode[] SolveAssignmentExpression(AssignmentExpressionSyntax assignmentNode)
        {
            return new[] { assignmentNode.Right };
        }

        public SyntaxNode[] SolveVariableDeclarator(VariableDeclaratorSyntax variableDeclaratorNode)
        {
            var eq = variableDeclaratorNode.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

            return eq?.ChildNodes().ToArray();
        }

        public SyntaxNode[] FindOrigin(MethodDeclarationSyntax rootNode, SyntaxNode currentNode, MethodScanResult result, List<SyntaxNode> visitedNodes, int level, Tainted tainted = null)
        {
            string arg = currentNode.ToString();
            int currentNodePosition = currentNode.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (currentNode is ArgumentSyntax)
            {
                if (currentNode.ChildNodes().First() is MemberAccessExpressionSyntax)
                    arg = currentNode.ChildNodes().First().ChildNodes().OfType<IdentifierNameSyntax>().First().Identifier.Text;
            }

            InvocationExpressionSyntax invocation = currentNode.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation != null)
            {
                return new SyntaxNode[] { invocation };
            }

            ConditionalExpressionSyntax conditional = currentNode.DescendantNodes().OfType<ConditionalExpressionSyntax>().FirstOrDefault();
            if (conditional != null)
            {
                return new SyntaxNode[] { conditional };
            }

            ObjectCreationExpressionSyntax objectCreation = currentNode.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().FirstOrDefault();
            if (objectCreation != null)
            {
                return new SyntaxNode[] { objectCreation };
            }

            foreach (AssignmentExpressionSyntax assignment in rootNode.DescendantNodes().OfType<AssignmentExpressionSyntax>().Where(a => a.Left.ToString() == arg).Reverse())
            {
                if (!visitedNodes.Contains(assignment) && currentNodePosition > assignment.GetLocation().GetLineSpan().StartLinePosition.Line)
                {
                    return new SyntaxNode[] { assignment };
                }
            }

            foreach (VariableDeclaratorSyntax dec in rootNode.DescendantNodes().OfType<VariableDeclaratorSyntax>().Where(d => d.Identifier.Text == arg).Reverse())
            {
                if (!visitedNodes.Contains(dec) && currentNodePosition > dec.GetLocation().GetLineSpan().StartLinePosition.Line)
                {
                    return new SyntaxNode[] { dec };
                }
            }

            // only after no assignment, declaration,... was found, only after that test for presence among parameters
            for (int i = 0; i < rootNode.ParameterList.Parameters.Count(); i++)
            {
                if (arg != rootNode.ParameterList.Parameters[i].Identifier.Text) continue;
                result.AppendEvidence(new string('-', (level - 2) * 2) + "> ^^^ BAD (Parameter)");
                result.Hits++;
                if (tainted != null) tainted.TaintedMethodParameters[i] += 1;
            }
            return null;
        }

        public async Task<SyntaxNode[]> SolveConditionalExpression(ConditionalExpressionSyntax currentNode, MethodScanResult result, int level)
        {
            try
            {
                bool evaluationResult = (bool)await CSharpScript.EvaluateAsync(currentNode.Condition.ToString());
                if (evaluationResult)
                {
                    result.AppendEvidence(new string(' ', level * 2) + "successfully evaluated condition as True (only 1 block will be investigated).");
                    return new SyntaxNode[1] { currentNode.WhenTrue };
                }
                else
                {
                    result.AppendEvidence(new string(' ', level * 2) + "successfully evaluated condition as False (only 1 block will be investigated).");
                    return new SyntaxNode[1] { currentNode.WhenFalse };
                }
            }
            catch (CompilationErrorException)
            {
                // unable to evaluate the condition, therefore both blocks of conditional expression have to be investigated
                result.AppendEvidence(new string(' ', level * 2) + "unsuccessfully evaluated condition (both blocks will be investigated).");
                return new SyntaxNode[2] { currentNode.WhenTrue, currentNode.WhenFalse };
            }
        }

        public void SolveLiteralExpression(MethodScanResult result, int level)
        {
            result.AppendEvidence(new string(' ', level * 2) + "OK (Literal)");
        }

        public void SolveUnrecognizedSyntaxNode(MethodScanResult result, SyntaxNode currentNode, int level)
        {
            result.AppendEvidence(new string(' ', level * 2) + "UNRECOGNIZED NODE " + currentNode.ToString());
        }
    }
}