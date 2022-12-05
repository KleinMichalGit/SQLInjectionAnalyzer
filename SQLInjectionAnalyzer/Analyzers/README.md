# Analyzers
The folder which contains all analyzers.

## Directory structure
- InterproceduralAnalyzer
- OneMethodAnalyzer
- SimpleAnalyzer


## Table of available analyzers
| Analyzer                      | Description                                                                                                                                                                                                                                                         |
|-------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Simple`                      | Reads *.cs files separately, without compiling .csproj files, without performing interprocedural analysis, every block of code is considered as reachable (very fast but very imprecise). Able to analyse 100 thousand+ C# files repository in a couple of minutes. |
| `OneMethod`                   | Compiles *.csproj files, without performing interprocedural analysis. Serves only for figuring out how much time is needed to compile all *.csproj files under the specified directory.                                                                             |
| `Interprocedural`             | Compiles *.csproj files, performs n-level interprocedural analysis, every block of code is considered as reachable.                                                                                                                                                 |
| `InterproceduralReachability` | Compiles *.csproj files, performs n-level interprocedural analysis, able to decide trivial problems when solving reachability problems (requires the most resources, the most precise type of analysis).                                                            |


## How to create your own analyzer
1. go to SQLInjectionAnalyzer/Model/Scope.cs and add your own unique Scope value.
2. go to SQLInjectionAnalyzer/SQLInjectionAnalyzer/Analyzers/ and create your own folder. The folder must contain exactly two files. 1 C# file for the analyzer and one readme file where you briefly describe the philosophy of your tool.
3. go to SQLInjectionAnalyzer/SQLInjectionAnalyzer/Program.cs and add the case for your analyzer into the switch.
4. go to SQLInjectionAnalyzer/SQLInjectionAnalyzer/OutputManager/OutputGenerator.cs and add the case for your analyzer into the switch. Here you will probably want to create your own template, so you can do it in the folder
  SQLInjectionAnalyzer/SQLInjectionAnalyzer/OutputManager/RazorOutput/
 
