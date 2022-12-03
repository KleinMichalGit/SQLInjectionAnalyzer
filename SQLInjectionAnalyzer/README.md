# SQL Injection Analyzer
Main folder for analyzer platform, contains Program.cs with main method.

## Directory structure
- `InputManager/` - readers and validators of config file and input.
- `OutputManager/` - data extractor, output generator, and individual templates for creating output according to Scope of analysis.
- `Analyzer.cs` - public abstract class which has to be inherited by every single analyzer. It contains only one ScanDirectory method, which takes information received on input and in config file, and returns Diagnostics object. The way how each analyzer implements this method is completely up to a derived class.  
- `CommonSyntaxHelper.cs` - contains helpful methods for working with syntax. Its methods are used by individual analyzers.  
- `InterproceduralAnalyzer.cs` - compiles *.csproj files, performs n-level interprocedural analysis, every block of code is considered as reachable.
- `OneMethodAnalyzer.cs` - compiles *.csproj files, without performing interprocedural analysis. The purpose of this analyzer is merely for understanding how long it takes to compile needed csproj files when analyzing extensive repository.
- `Program.cs` - contains Main method, start of program. 
- `SimpleAnalyzer.cs` - reads *.cs files separately, without compiling .csproj files, without performing interprocedural analysis, every block of code is considered as reachable (very fast but very imprecise).

## Adding your own analyzers
TODO guide about how to add your own analyzer