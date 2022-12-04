# Simple Analyzer

## A brief description of philosophy
TODO

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
