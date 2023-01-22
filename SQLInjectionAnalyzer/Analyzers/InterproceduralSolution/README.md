# InterproceduralSolution Analyzer

## A brief description of philosophy
This implementation goes one step beyond InterproceduralCSProj analyzer. It is able to search for the callers across
the entire solution file (.sln). It opens all C# solution (*.sln) files, for each solution creates a compilation 
containing all C# projects (.csproj), for each project performs n-level interprocedural analysis
(where number n is defined in config.json file), able to decide trivial conditional statements.

## Solution analysis
The main part of functionality takes place in the method `InterproceduralSolutionScanMethod`.

## Example
for the following example, the invocation of `SinkMethod` with unprocessed arguments is considered as potentially vulnerable.

#### class ConsoleApp in ConsoleApp1 namespace
```cs
namespace ConsoleApp1
{
    public class ConsoleApp
    {
        public void ThisIsVulnerableMethod(string argument)
        {
            SinkMethod(argument);
        }

        public void SinkMethod(string arg)
        {

        }

        private void ICallYou()
        {
            ThisIsVulnerableMethod("");
        }

    }
}
```
#### class Program in SolutionForTestingInterproceduralAnalyzers namespace 
```cs
using ClassLibrary1;

namespace SolutionForTestingInterproceduralAnalyzers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
        }

        public void A(string arg)
        {
            ConsoleApp consoleApp = new ConsoleApp();

            consoleApp.ThisIsVulnerableMethod(arg);
        }

        public void B(string x)
        {
            ConsoleApp consoleApp = new ConsoleApp();

            consoleApp.ThisIsVulnerableMethod("");
        }

        private void C(string x)
        {
            ConsoleApp consoleApp = new ConsoleApp();

            consoleApp.ThisIsVulnerableMethod(x);
        }
    }
}
```
## The result of InterprocedualSolution analysis
```
currently scanned .sln: C:\Users\kleinmichal\SolutionForTestingInterproceduralAnalyzers\SolutionForTestingInterproceduralAnalyzers.sln
0 / 1 .sln files scanned
    + project: C:\Users\kleinmichal\SolutionForTestingInterproceduralAnalyzers\SolutionForTestingInterproceduralAnalyzers\SolutionForTestingInterproceduralAnalyzers.csproj
    + project: C:\Users\kleinmichal\SolutionForTestingInterproceduralAnalyzers\ClassLibrary1\ClassLibrary1.csproj
-----------------------
Vulnerable method found
Method name: ThisIsVulnerableMethod(string argument)
-----------------------
Interprocedural callers tree:
level | method
1   ClassLibrary1.ConsoleApp.ThisIsVulnerableMethod(string)
2     SolutionForTestingInterproceduralAnalyzers.Program.A(string)
2     SolutionForTestingInterproceduralAnalyzers.Program.B(string)
2     SolutionForTestingInterproceduralAnalyzers.Program.C(string)
2     ClassLibrary1.ConsoleApp.ICallYou()

-----------------------
Evidence:
INTERPROCEDURAL LEVEL: 1
SinkMethod(argument)
  argument
> ^^^ BAD (Parameter)
INTERPROCEDURAL LEVEL: 2 SolutionForTestingInterproceduralAnalyzers.Program.A(string)
consoleApp.ThisIsVulnerableMethod(arg)
  arg
> ^^^ BAD (Parameter)
INTERPROCEDURAL LEVEL: 2 SolutionForTestingInterproceduralAnalyzers.Program.B(string)
consoleApp.ThisIsVulnerableMethod("")
  ""
ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.
INTERPROCEDURAL LEVEL: 2 SolutionForTestingInterproceduralAnalyzers.Program.C(string)
consoleApp.ThisIsVulnerableMethod(x)
  x
> ^^^ BAD (Parameter)
INTERPROCEDURAL LEVEL: 2 ClassLibrary1.ConsoleApp.ICallYou()
ThisIsVulnerableMethod("")
  ""
ALL TAINTED VARIABLES CLEANED IN THIS BRANCH.
ON THIS LEVEL OF INTERPROCEDURAL ANALYSIS, THERE IS AT LEAST ONE METHOD WITH TAINTED PARAMETERS WITHOUT ANY CALLERS. THEREFORE, ITS PARAMETERS ARE UNCLEANABLE.

-----------------------
1 / 1 .sln files scanned
-----------------------------
Analysis start time: 22. 1. 2023 17:03:04
Analysis end time: 22. 1. 2023 17:03:07
Analysis total time: 00:00:02.7990468
Scanned methods: 1
Skipped methods: 2
Number of all sink invocations: 1
Vulnerable methods: 1
-----------------------------
```

From the result we can see that n-level interprocedual callers tree contains callers from different csproj files.