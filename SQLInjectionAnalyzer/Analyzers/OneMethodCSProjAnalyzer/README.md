# OneMethodCSProj Analyzer

## A brief description of philosophy
Compiles *.csproj files, without performing interprocedural analysis.
Uses the same rules as OneMethodSyntaxTree, therefore provides the same results. 
This ScopeOfAnalysis serves only to investigate how much time is needed for compilation of all .csproj files.
Able to decide trivial conditional statements.
Actually, the compilation of *.csproj files takes the most of the time of the analysis. 
Instead of analysing each *.cs file one-by-one, we use `MSBuildWorkspace`
to open project `Project` using method `OpenProjectAsync`. 
We receive compilation `Compilation` using method `GetCompilationAsync`.
The compilation contains the syntax trees parsed from source code the compilation was created with. 
After that, we scan each syntax tree using method `ScanSyntaxTree`.

## Taint propagation rules
One MethodCSProj Analyzer shares the same Taint propagation variable rules with OneMethodSyntaxTree analyzer.

#### Table of rules
| Node                             | Rule                                           |
|----------------------------------|------------------------------------------------|
| `InvocationExpressionSyntax`     | `SolveInvocationExpression`                    |
| `ObjectCreationExpressionSyntax` | `SolveObjectCreationExpression`                |
| `AssignmentExpressionSyntax`     | `SolveAssignmentExpression`                    |
| `VariableDeclaratorSyntax`       | `SolveVariableDeclarator`                      |
| `ArgumentSyntax`                 | `FindOrigin`                                   |
| `IdentifierNameSyntax`           | `FindOrigin`                                   |
| `ConditionalExpressionSyntax`    | `SolveConditionalExpression`                   |
| `LiteralExpressionSyntax`        | `SolveLiteralExpression`                       |
| else                             | `SolveUnrecognizedSyntaxNode`                              |

#### Description of rules
- `SolveInvocationExpression`
  - if the current node, which is an invocation expression, represents the calling of the method which belongs to the set of cleaningMethods: consider the current node as clean and stop tracking the origin of this node.
  - otherwise, solve one-by-one from left to right each of the method arguments `ArgumentSyntax` from `ArgumentList.Arguments` (Depth-first approach).
- `SolveObjectCreationExpression` - solve one-by-one from left to right each of the object creation arguments `ArgumentSyntax` from `ArgumentList.Arguments` (Depth-first approach).
- `SolveAssignmentExpression` - solve the expression located right to the equality sign (=).
- `SolveVariableDeclarator` - solve the expression located right to the equality sign (=).
- `FindOrigin` - find origin of argument or identifier:
  - if the current node is `InvocationExpressionSyntax`, then `SolveInvocationExpression`.
  - if the current node is `ConditionalExpressionSyntax`, then `SolveConditionalExpression`.
  - if the current node is `ObjectCreationExpressionSyntax`, then `SolveObjectCreationExpression`
  - if none of the above is true, then try to find the last unvisited `AssignmentExpressionSyntax` located above the current node, where there is a value assigned to this node (search the SyntaxTree in post order).
  - if no assignment has been discovered, try to find the last unvisited `VariableDeclaratorSyntax` located above the current node, where the node was declared (search the SyntaxTree in post order).
  - if no assignment nor declaration were found, look at the parameters of the currently analysed method. If there is a parameter representing the origin of the currently analysed node, it is a bad parameter (it comes from the outside). Therefore, append "BAD (Parameter)" to the evidence and stop solving this node.
- `SolveConditionalExpression` - try to evaluate the `Condition` of `ConditionalExpressionSyntax`. If the evaluation was successful, solve the expression in `WhenTrue` block, otherwise solve the expression in `WhenFalse`. If the evaluation was unsuccessful (for example due to missing context), solve both blocks `WhenTrue` and `WhenFalse`.
- `SolveLiteralExpression` - append "OK (Literal)" to the evidence and stop solving the current node.
- `SolveUnrecognizedSyntaxNode` - append "UNRECOGNIZED NODE" to the evidence and stop solving the current node.