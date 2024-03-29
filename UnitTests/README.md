# Unit Tests
Unit tests for the rules in the table of rules, config file reader and input reader. E2E tests for OneMethodSyntaxTreeAnalyzer. 
In this folder, there are also helpful tools for creating own unit tests.

## How to add your own Unit Tests
After creating your own implementation of an analyzer, or after modifying the existing
functionality, please make sure the additional or changed functionality is
fully covered by `unit tests`. To inspire yourself, look at [OneMethodSyntaxTreeTest.cs](OneMethodSyntaxTreeAnalyzerTest.cs)
which covers the functionality of the implementation of the analyzer for OneMethodSyntaxTree scope of analysis.
Another examples are [ConfigFileReaderTest.cs](ConfigFileReaderTest.cs) or [InputReaderTest.cs](InputReaderTest.cs)
which test [ConfigFileReader](../InputService/ConfigFileReader.cs) and [InputReader](../InputService/InputReader.cs)
in InputService. You can add additional tests right here into this folder.

### ScenarioFactory.cs
[ScenarioFactory.cs](ScenarioFactory.cs) contains useful methods for creating `Test Scenarios`, and for comparing
expected and actual `Diagnostics`. For example `TwoDiagnosticFilesShouldBeEqual(Diagnostics expected, Diagnostics actual)`.
Since `Diagnostics` contain `time information` it would be impossible (and it would make no sense) to compare time values because
every time they can be different (it depends on the environment where tests are run). Therefore, only values which `do not`
depend on the environment are tested for equality. For example `ScopeOfAnalysis`, `NumberOfCSProjFiles`, or `CSProjectScanResults` should be equal. 
#### Create scenario
To create your own `Test Scenario`, call `CreateScenario(TaintPropagationRules rules, string directoryPath, Diagnostics expectedDiagnostics, List<string> excludeSubpaths)`,
where `rules` are the rules you want to use for solving the taint variable propagation equations, `directoryPath` is a directory path to code which should be analysed, `expectedDiagnostics` are `Diagnostics` which
you expect to receive from the analysis, and `excludeSubpaths` are the sub-paths which you want to skip during the analysis. The `Scenario`
compares by `asserting` actual results, and expected results. The test scenario is successful if actual results are equal to expected results, otherwise it fails.

### TableOfRulesTest.cs
Tests each rule for solving taint variable propagation problems separately.

### NodeFactory.cs
Finds an `index-th` syntactic node in a file with a path `filePath`. Use it by calling a `T FindSyntaxNode<T>(string filePath, int index)`. 

### Config file examples
`ConfigFileExamples/` folder contains both valid and invalid config files for testing. Feel free to use them, or create your own.

### Code to be analysed
`CodeToBeAnalysed/` folder contains the code for analysis for `unit testing`. Since this repository is a `static source code analyzer`, to test such
code we need some code which will serve as a material for unit tests.

### Expected diagnostics
`ExpectedDiagnostics/` folder contains many `Diagnostics` which are used as `expectedDiagnostics` for `Test Scenario`.

### Taint propagation rules examples
`TaintPropagationRulesExamples` folder contains a taint propagation rules creator which creates custom rules for solving 
taint variable propagation equations.

## How to run the tests
#### Test Explorer
Please use Test Explorer in Visual Studio for running Unit Tests.
![img_1.png](Images/img_1.png)
The good practice is to always attach the evidence/output of the test into Test Detail Summary.
![img.png](Images/img.png)

## Directory structure
- `CodeToBeAnalysed/` - the code which serves as a source code which should be analysed during unit tests. Contains all examples and use cases covered by unit tests. Mainly every single taint propagation rule contains source code designed specifically to test the functionality of that rule. Contains both safe and vulnerable cases.  
- `ConfigFileExamples/` - valid and invalid config files tested by unit tests.
- `ExpectedDiagnostics/` - expected diagnostics of each tested analyzer. The results of unit tests are compared against these expected diagnostics. Tests are successful only if expected and received diagnostics are considered as equal. As diagnostics also contain various measured times, these times are not compared and should not be compared as it makes no logical sense comparing the times of two otherwise identical diagnostics (it depends on environment, etc..).   
- `TaintPropagationRulesExamples/` -  contains a creator of taint propagation rules used by unit tests.
- `ScenarioFactory.cs` - contains helpful methods for creating test scenarios, and comparing diagnostic files. 
- `ConfigFileReaderTest.cs` - tests for config file reader.
- `InputReaderTest.cs` - tests for reading and validating input.
- `OneMethodSyntaxTreeTest.cs` - tests for OneMethodSyntaxTree analyzer.
- `NodeFactory.cs` - finds a syntactic node in a file. The node is then used for testing the rules in the table of rules.
- `TableOfRules.cs` - individual tests for each rule.
