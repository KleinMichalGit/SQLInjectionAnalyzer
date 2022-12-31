# Naming conventions
Here are listed some recommended naming conventions.

## Table of available analyzers
| 👍 Do ✅     | 👎 Don't ❌ | 🧠 Reason                                                                                                                                                                                                                                                 |
|-------------|------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| analyzer    | analyser   | Since [Roslyn](https://github.com/dotnet/roslyn "The .NET Compiler Platform") uses American english to name `analyzers` (with 'z'), and analyzer in this repository is Roslyn-based, we should also follow this convention and name `Analyzers` with 'z'. |
| analysis    | analyzis   | Since the word 'analyzis' does not exist, we should always use 'analysis' when referring to the process of investigating the code. For example: `Static Source Code Analysis`.                                                                            |
| diagnostics | -          | An object which contains all information gained during the process of analysis.                                                                                                                                                                           |
| output      | -          | A file (or a report) generated from diagnostics. It can be either `.html` file, `.txt` file, or simply a real-time writing on standard output (console).                                                                                                  |
