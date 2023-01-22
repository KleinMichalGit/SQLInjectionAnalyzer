# SQL Injection Analyzer
Main folder for analyzer platform, contains `Program.cs` with `Main` method.
To add your own implementations of an analyzer, go to `Analyzers/`.

## Directory structure
- [Analyzer.cs](Analyzer.cs) - public abstract class which has to be inherited by every single analyzer. It contains only one ScanDirectory method, which takes information received on input and in config file, and returns Diagnostics object. The way how each analyzer implements this method is completely up to a derived class.
- `Analyzers/` - a folder which contains a separate implementations of `Analyzer.cs`.
- [GlobalHelper.cs](GlobalHelper.cs) - contains helpful methods for searching and deciding. Its methods are used by individual implementations of analyzer.
- [InterproceduralHelper.cs](InterproceduralHelper.cs) - contains methods for interprocedural analyzers.
- [Program.cs](Program.cs) - contains `Main` method. Defines the entry point of the application.
- [DiagnosticsInitializer.cs](DiagnosticsInitializer.cs) - contains methods for initializing diagnostics.
