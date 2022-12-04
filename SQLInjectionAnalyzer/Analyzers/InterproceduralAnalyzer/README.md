# Interprocedural Analyzer

## A brief description of philosophy
Compiles *.csproj files, performs n-level interprocedural analysis, every block of code is considered as reachable.
In comparison to OneMethod analyzer, Interprocedural analyzer is able to search for the callers of the currently analysed method
among every single C# file mentioned in the currently analysed .csproj file. It does so by creating BFS tree of callers-callees
with the maximal height of `n`. The number `n` is defined in the `config.json` file.


## Taint propagation rules

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
| else                             | Unrecognized node                              |

#### Description of rules
- `SolveInvocationExpression`
    - if the current node, which is an invocation expression, represents the calling of the method which belongs to the set of cleaningMethods: consider the current node as clean and stop solving this node.
    - otherwise, solve one-by-one from left to right each of the arguments `ArgumentSyntax` (Depth-first approach).
- `SolveObjectCreationExpression` - solve one-by-one from left to right each of the arguments `ArgumentSyntax`.
- `SolveAssignmentExpression` - consider only the part behind the equal sign (=). Solve one-by-one from left to right each of the identifiers `IdentifierNameSyntax`.
- `SolveVariableDeclarator` - consider only the part behind the equal sign (=). Solve one-by-one every child node in prefix document order.
- `FindOrigin` - find origin of argument or identifier. If the current node is either argument or identifier,
    - if the current node is `InvocationExpressionSyntax`, then `SolveInvocationExpression`.
    - if the current node is `ConditionalExpressionSyntax`, then `SolveConditionalExpression`.
    - if the current node is `ObjectCreationExpressionSyntax`, then `SolveObjectCreationExpression`
    - if none of the above is true, then try to find the last unvisited `AssignmentExpressionSyntax` where there is a value assigned to this node (in post order).
    - if no assignment has been discovered, try to find the last unvisited `VariableDeclaratorSyntax` where the node was declared (in post order).
    - if no assignment nor declaration has been found, look at the parameters of the currently analysed method. If there is a parameter representing the origin of the currently analysed node, it is a bad parameter (it comes from the outside). Therefore, append "BAD (Parameter)" to the evidence and stop solving this node.
- `SolveConditionalExpression` - find origin one-by-one from left to right for each identifier `IdentifierNameSyntax`.
- `SolveLiteralExpression` - append "OK (Literal)" to the evidence and stop solving the current node.
- else - append "UNRECOGNIZED NODE" to the evidence and stop solving the current node.