# Analyzers
The folder which contains all analyzer implementations.

## Directory structure
- [InterproceduralCSProjAnalyzer](InterproceduralCSProjAnalyzer/InterproceduralCSProjAnalyzer.cs).
- [OneMethodCSProjAnalyzer](OneMethodCSProjAnalyzer/OneMethodCSProjAnalyzer.cs).
- [OneMethodSyntaxTreeAnalyzer](OneMethodSyntaxTreeAnalyzer/OneMethodSyntaxTreeAnalyzer.cs).
- [TableOfRules](TableOfRules.cs) - rules for tracking taint propagation variables.

## Table of available analyzers
| Analyzer                      | Description                                                                                                                                                                                                                                                                                                                |
|-------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `OneMethodSyntaxTree`                      | Reads C# (*.cs) files separately and investigates Syntax Trees parsed from the separate C# files, without compiling .csproj files, without performing interprocedural analysis, able to decide trivial conditional statements (very fast but very inacurate).                                                              |
| `OneMethodCSProj`                   | Compiles *.csproj files, without performing interprocedural analysis. Uses the same rules as OneMethodSyntaxTree, therefore provides the same results. This ScopeOfAnalysis serves only to investigate how much time is needed for compilation of all .csproj files. Able to decide trivial conditional statements         |
| `InterproceduralCSProj`             | Compiles all C# project (*.csproj) files, performs n-level interprocedural analysis (where number n is defined in config.json file) for each project separately, able to decide trivial conditional statements.                                                                                                            |
| `InterproceduralSolution` | Opens all C# solution (*.sln) files, performs n-level interprocedural analysis (where number n is defined in config.json file) for each solution separately, able to decide trivial conditional statements.                                                                                           |
| `InterproceduralOneSolution` | Creates 1 universal C# solution (*.sln) by compiling all C# project files (.csproj) and referrencing them in the solution, performs n-level interprocedural analysis (where number n is defined in config.json file) at 1 universaly created solution, able to decide trivial conditional statements |

## How to create your own analyzer 
1. go to SQLInjectionAnalyzer/Model/Scope.cs and add your own unique Scope of analysis value.
2. go to SQLInjectionAnalyzer/SQLInjectionAnalyzer/Analyzers/ and create your own folder. The folder must contain exactly two files. 1 C# file for the analyzer and one readme file where you briefly describe the philosophy of your tool.
3. go to SQLInjectionAnalyzer/SQLInjectionAnalyzer/Program.cs and add the case for your analyzer into the switch.
4. go to SQLInjectionAnalyzer/OutputService/OutputGenerator.cs and add the case for your analyzer into the switch. Here you will probably want to create your own template, so you can do it in the folder
  SQLInjectionAnalyzer/OutputService/RazorOutput/
 
