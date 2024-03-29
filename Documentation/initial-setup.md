# Initial setup

## Clone
Make sure you have enough free disk space.
Building SQLInjectionAnalyzer involves downloading many packages. The recommendation is to dedicate at
least 700 MB of free disk space for the repository.
#### HTTPS
```
git clone https://github.com/KleinMichalGit/SQLInjectionAnalyzer.git
```
#### SSH
```
git clone git@github.com:KleinMichalGit/SQLInjectionAnalyzer.git
```
#### GitHub CLI
```
gh repo clone KleinMichalGit/SQLInjectionAnalyzer
```
## Build prerequisites
#### Visual Studio 2019, version 16.8.3 or higher
Make sure you have Visual Studio 2019, version 16.8.3 or higher installed on your machine.

## One click/line build
![img.png](images/build.png)

Run either `Build Solution` in your Visual Studio, or
```
dotnet build
```
in command line.

### Exemplary usage
``` shell
.\SQLInjectionAnalyzer.exe --path=.\source\folder\ --scope-of-analysis=InterproceduralCSProj --config=.\config\folder\config.json --result=.\result\path\ --exclude-paths=TEST,E2E --write-console
```

### Arguments
```
--path=VALUE                 (MANDATORY) path to the folder which should be analysed
--scope-of-analysis=VALUE    (MANDATORY) determines the scope of analysis
--config=VALUE               (MANDATORY) path to .json config file
--result=VALUE               (MANDATORY) path to the folder where diagnostic-result-files should be exported
--exclude-paths=VALUE        (OPTIONAL)  comma delimited list of sub-paths to be skipped during analysis
--write-console              (OPTIONAL)  write real-time diagnostic-results on console during analysis
--help                                   show this usage tutorial and exit
```

### About arguments
```
--path:
     any valid path to the folder which should be analysed.
--scope-of-analysis:
     OneMethodSyntaxTree           Reads C# (*.cs) files separately and investigates Syntax Trees parsed from the separate C# files,
                                   without compiling .csproj files, without performing interprocedural analysis, able to decide trivial
                                   conditional statements (very fast but very inacurate).
     OneMethodCSProj               Compiles *.csproj files, without performing interprocedural analysis. Uses the same rules as
                                   OneMethodSyntaxTree, therefore provides the same results. This ScopeOfAnalysis
                                   serves only to investigate how much time is needed for compilation of all .csproj files.
                                   Able to decide trivial conditional statements.
     InterproceduralCSProj         Compiles all C# project (*.csproj) files, performs n-level interprocedural analysis (where number n is defined
                                   in config.json file) for each project separately, able to decide trivial conditional statements.
     InterproceduralSolution       Opens all C# solution (*.sln) files, performs n-level interprocedural analysis (where number n is
                                   defined in config.json file) for each solution separately, able to decide trivial conditional statements.
--config:
     any valid path to valid config.json (configures rules for taint variables propagation).
--result:
     any valid path to the folder where diagnostic-result-files should be exported.
--exclude-paths:
     comma delimited list of sub-paths to be skipped during analysis (for example tests).
--write-console:
     informs about results in real-time.
```
## Configuration
The file which specifies configuration rules for solving taint variable propagation problems is expected to have the following format.
It must be `*.json` file.
- level - maximal allowed height of BFS tree during `Interprocedural` analysis
- sourceAreas - badges for method findings which should be added to the `.html` result file. `label` defines the badge which should be added, path defines the path of the file containing at least one method analysed during analysis.
- sinkMethods - the names of the methods considered to be potentially dangerous when any non-parametrised parameter is passed to them.
- cleaningMethods - the names of the methods considered to be clear. Therefore, if any tainted variable is passed to the calling of such method, it will automatically clean the tainted variable.

#### Exemplary `config.json` file:
```json
{
  "level": 3,
  "sourceAreas": [
    {
      "label": "WEB",
      "path": "my\\path\\web\\"
    },
    {
      "label": "DATABASE",
      "path": "another\\path\\database\\"
    }
  ],
  "sinkMethods": [
    "NameOfTheSinkMethod1",
    "NameOfTheSinkMethod2"
  ],
  "cleaningMethods": [
    "NameOfTheCleaningMethod1",
    "NameOfTheCleaningMethod2"
  ]
}
```
