# Unit Tests
Unit tests for all types of analyzers, config file reader and input reader.

## Directory structure
- `CodeToBeAnalysed/` - the code which serves as a source code which should be analysed during unit tests. Contains all examples and use cases covered by unit tests. Mainly every single taint propagation rule contains source code designed specifically to test the functionality of that rule. Contains both safe and vulnerable cases.  
- `ConfigFileExamples/` - valid and invalid config files tested by unit tests.
- `ExpectedDiagnostics/` - expected diagnostics of each tested analyzer. The results of unit tests are compared against these expected diagnostics. Tests are successful only if expected and received diagnostics are considered as equal. As diagnostics also contain various measured times, these times are not compared and should not be compared as it makes no logical sense comparing the times of two otherwise identical diagnostics (it depends on environment, etc..).   
- `TaintPropagationRulesExamples/` -  contains a creator of taint propagation rules used by unit tests.
- `AnalyzerTestHelper.cs` - contains helpful methods for creating test scenarios, and comparing diagnostic files. 
- `ConfigFileReaderTest.cs` - tests for config file reader.
- `SimpleAnalyzerTest.cs` - tests for simple analyzer.

## Test Explorer
Please use Test Explorer in Visual Studio for running Unit Tests.
![img_1.png](img_1.png)
The good practice is to always attach the evidence/output of the test into Test Detail Summary.
![img.png](img.png)