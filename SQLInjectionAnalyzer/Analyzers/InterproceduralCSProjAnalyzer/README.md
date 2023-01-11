# InterproceduralCSProj Analyzer

## A brief description of philosophy
Compiles all C# project (*.csproj) files, performs n-level interprocedural analysis 
(where number n is defined in config.json file) for each project separately, able 
to decide trivial problems when solving reachability problems.
In comparison to OneMethod analyzers, InterproceduralCSProj analyzer is able to search for the callers of 
the currently analysed method among every single C# file referred to in the currently analysed .csproj file.
It does so by creating BFS tree of callers-callees with the maximal height of `n`.
The number `n` is defined in the `config.json` file.

## Interprocedural Analysis
By far, the most interesting functionality of this analyzer is how callers-callees are found on
interprocedural scope. The following list describes the methods used during this process.

- `SolveInterproceduralAnalysis` - `n`-times iterates through each `SyntaxTree` of `Compilation`. On each level, finds all method invocations of the currently analysed method inside all SyntaxTrees.
For each invocation find out if it is located inside the body of the method. If yes, solve the invocation as if it is the invocation of the
method from the set of sinkMethods. However, only those invocation arguments should be solved which were considered as tainted on previous level.
    - for each level of BFS tree, we append "INTERPROCEDURAL LEVEL: " + currentLevel + " " + semanticModel.GetDeclaredSymbol(parent).ToString() into the evidence.
    - if all taint variables are cleaned in the branch, we append "ALL TAINTED VARIABLES CLEANED IN THIS BRANCH." into the evidence.
    - if there is a method which has tainted parameters but there are no callers of this method in any C# file in the current .csproj, we add "ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, THERE IS AT LEAST ONE METHOD WITH TAINTED PARAMETERS WHICH DOES NOT HAVE ANY CALLERS. THEREFORE ITS PARAMETERS ARE UNCLEANABLE." into the evidence. Since the parameters are uncleanable, this method must be included among the final results.
    - if all taint variables are cleaned on the entire level of BFS tree (in all branches), we append "ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, ALL TAINTED VARIABLES WERE CLEANED. THEREFORE, THIS MESSAGE SHOULD NOT BE INCLUDED AMONG RESULTS." into the evidence.
    - if there is a caller of the method, but it has a different number of arguments, we append "THERE IS A CALLER OF METHOD " + block.MethodSymbol.ToString() + " BUT WITH DIFFERENT AMOUNT OF ARGUMENTS (UNABLE TO DECIDE WHICH TAINTED ARGUMENT IS WHICH)" 
- `CurrentLevelContainsTaintedBlocksWithoutCallers` - decides whether the current level of BFS tree contains methods with tainted parameters without callers.
- `AllTaintVariablesAreCleanedInThisBranch` - decides whether in this branch on this level of BFS tree all tainted variables are cleaned.
- `FindAllCallersOfCurrentBlock` - finds all callers of the method with the same number of arguments as the method's parameters. 
- `SolveSourceAreas` - handles adding the badges specified in the config file.
- `FindMethodParent` - tries to figure out if the current `SyntaxTree` node has `MethodDeclarationSyntax` parent or not.

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

## Example
Consider the two following C# files. The first one:
```cs
using System;

namespace InterproceduralCodeToBeAnalysed
{
    class C1
    {
        public void A(string s, string q)
        {
            string arg1 = s;
            int arg2;

            arg1 = CreateStringValue(1 < 2 ? arg1 : "string literal");
            arg2 = 0;
            arg2 = 1;
            arg2 = 2;

            SinkMethod(arg1, arg2);
        }

        private string CreateStringValue(string a)
        {
            return a;
        }
        
        private void SinkMethod(string a, int b)
        {
        }
    }

    class C2
    {
        string myString = new C1().A("", "");

        public void B(string s)
        {
            new C1().A("", s);
        }

        public void C(string s)
        {
            new C1().A(s, "");
        }

        void E()
        {
            C("");
        }

    }
}

```
The second one:
```cs
namespace InterproceduralCodeToBeAnalysed
{
    public class C3
    {

        public void X(string s)
        {
            new C1().A(s, "");
        }

        private void V(string s)
        {
            X(s);
        }
    }
}
```
Also, consider the following config file:
```json
{
    "level": 3,
    "sourceAreas": [],
    "sinkMethods": [
        "SinkMethod"
    ],
    "cleaningMethods": []
}
```
Interprocedural analysis of these two files using the config.json above would produce the following evidence:
```
currently scanned .csproj: .\path\InterproceduralCodeToBeAnalysed.csproj
0 / 1 .csproj files scanned
-----------------------
Vulnerable method found
Method name: A
-----------------------
Interprocedural callers tree:
level | method
1   InterproceduralCodeToBeAnalysed.C1.A(string, string)
2     InterproceduralCodeToBeAnalysed.C2.B(string)
2     InterproceduralCodeToBeAnalysed.C2.C(string)
2     InterproceduralCodeToBeAnalysed.C3.X(string)
3       InterproceduralCodeToBeAnalysed.C2.E()
3       InterproceduralCodeToBeAnalysed.C3.V(string)

-----------------------
Evidence:
INTERPROCEDURAL LEVEL: 1
SinkMethod(arg1, arg2)
    arg1
        arg1 = CreateStringValue(1 < 2 ? arg1 : "string literal")
          CreateStringValue(1 < 2 ? arg1 : "string literal")
              1 < 2 ? arg1 : "string literal"
                  1 < 2
                    UNRECOGNIZED NODE 1 < 2
                  arg1
                      arg1 = s
                          s
------------------------> ^^^ BAD (Parameter)
                  "string literal"
                    OK (Literal)
    arg2
        arg2 = 2
          2
INTERPROCEDURAL LEVEL: 2 InterproceduralCodeToBeAnalysed.C2.B(string)
new C1().A("", s)
    ""
ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.
INTERPROCEDURAL LEVEL: 2 InterproceduralCodeToBeAnalysed.C2.C(string)
new C1().A(s, "")
    s
--> ^^^ BAD (Parameter)
INTERPROCEDURAL LEVEL: 2 InterproceduralCodeToBeAnalysed.C3.X(string)
new C1().A(s, "")
    s
--> ^^^ BAD (Parameter)
INTERPROCEDURAL LEVEL: 3 InterproceduralCodeToBeAnalysed.C2.E()
C("")
    ""
ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.
INTERPROCEDURAL LEVEL: 3 InterproceduralCodeToBeAnalysed.C3.V(string)
X(s)
    s
--> ^^^ BAD (Parameter)

-----------------------
1 / 1 .csproj files scanned
-----------------------------
Analysis start time: 4. 12. 2022 22:55:11
Analysis end time: 4. 12. 2022 22:55:16
Analysis total time: 00:00:04.9897972
Scanned methods: 1
Skipped methods: 7
Number of all sink invocations: 1
Vulnerable methods: 1
-----------------------------
```