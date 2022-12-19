using System.Collections.Generic;
using Model.CSProject;
using Model.Method;
using Model.SyntaxTree;
using Model;

namespace UnitTests.ExpectedDiagnostics
{
    /// <summary>
    /// common helper for creating custom expected diagnostics <see cref="Diagnostics"/>
    /// for Simple scope of analysis <see cref="ScopeOfAnalysis"/>.
    /// </summary>
    public class ExpectedDiagnosticsSimpleAnalysis
    {
        
        public Diagnostics GetSimpleEmptyDiagnostics()
        {
            return new Diagnostics();
        }

        public Diagnostics GetSimpleDiagnosticsWithOneCSFileScaned()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() { "../../CodeToBeAnalysed/OneSafeMethod/SimpleClass.cs" },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetOneVulnerableMethodDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() { "../../CodeToBeAnalysed/OneVulnerableMethod/SimpleClass.cs" },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string args)\r\n        {\r\n            SinkMethodOne(args);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 3,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/CleaningRules/AllArgumentsAreCleaned/ArgumentsAreCleaned1.cs",
                            "../../CodeToBeAnalysed/CleaningRules/AllArgumentsAreCleaned/ArgumentsAreCleaned2.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0
                                    }
                                }
                            },
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetNotAllArgumentsAreCleanedDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/CleaningRules/NotAllArgumentsAreCleaned/NotAllArgumentsAreCleanedClass.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            const string thisIsConst = \"this is const\";\r\n\r\n            SinkMethodOne(thisIsConst, arg);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 5,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetInvocationRulesDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/InvocationRules/InvocationTestClass.cs",
                            "../../CodeToBeAnalysed/InvocationRules/InvocationTestClass2.cs",
                            "../../CodeToBeAnalysed/InvocationRules/InvocationTestClass3.cs",
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 2,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0,
                                    }
                                }
                            },
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 2,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0,
                                    }
                                }
                            },
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 2,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string arg1 = CreateStringValue(arg);\r\n            string arg2 = CreateStringValue(string.Empty);\r\n\r\n            string arg3 = CreateStringValue(arg1);\r\n            string arg4 = CreateStringValue(arg2);\r\n\r\n            SinkMethodOne(arg3, arg4);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 9,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetExcludingPathsDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/ExcludingPaths/ExcludingPaths1\\ThisClassWillBeScanned.cs",
                            "../../CodeToBeAnalysed/ExcludingPaths/ExcludingPaths2\\ThisClassWillNotBeScanned.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            const string thisIsConst = \"this is const\";\r\n\r\n            SinkMethodOne(thisIsConst, arg);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 5,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetSafeAssignmentsDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/AssignmentRules/SafeAssignment/SafeAssignment1.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetVulnerableAssignmentsDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/AssignmentRules/VulnerableAssignment/VulnerableAssignment1.cs",
                            "../../CodeToBeAnalysed/AssignmentRules/VulnerableAssignment/VulnerableAssignment2.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                            new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string vulnerableString1 = arg;\r\n            string vulnerableString2;\r\n            string vulnerableString3;\r\n            string vulnerableString4;\r\n\r\n            vulnerableString2 = vulnerableString1;\r\n            vulnerableString3 = vulnerableString2;\r\n            vulnerableString4 = vulnerableString3;\r\n\r\n            SinkMethodOne(vulnerableString4);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 12,
                                    }
                                }
                            },
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 2,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 2,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string vulnerableString1 = arg;\r\n            string vulnerableString2;\r\n            string vulnerableString3;\r\n            string vulnerableString4;\r\n\r\n            vulnerableString2 = 1 > 2 ? vulnerableString1 : arg;\r\n            vulnerableString3 = CreateString(vulnerableString2);\r\n            vulnerableString4 = vulnerableString3;\r\n\r\n            SinkMethodOne(vulnerableString4);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 12,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetSafeConditionalExpressionDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/ConditionalExpressionRules/SafeConditionalExpression\\SafeConditionalExpressionClass.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetVulnerableConditionalExpressionDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/ConditionalExpressionRules/VulnerableConditionalExpression\\VulnerableConditionalExpressionClass.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string thisIsConditionalExpression = 1 < 2 ? \"the universe works\" : arg;\r\n\r\n            SinkMethodOne(thisIsConditionalExpression);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 5,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        internal Diagnostics GetSafeObjectCreationDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/CreationRules/SafeCreationRules/SafeCreationClass.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 0,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        public Diagnostics GetVulnerableObjectCreationDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/CreationRules/VulnerableCreationRules/VulnerableCreationClass.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 1,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string myString = arg;\r\n            MyClass myClass = new MyClass(myString);\r\n\r\n            SinkMethodOne(myClass);\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 6,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        internal Diagnostics GetComplexTestDiagnostics()
        {
            return new Diagnostics()
            {
                CSProjectScanResults = new List<CSProjectScanResult>()
                {
                    new CSProjectScanResult()
                    {
                        NamesOfAllCSFilesInsideThisCSProject = new List<string>() {
                            "../../CodeToBeAnalysed/Complex/ComplexAnalysis1.cs"
                        },
                        SyntaxTreeScanResults = new List<SyntaxTreeScanResult>()
                        {
                             new SyntaxTreeScanResult()
                            {
                                NumberOfSkippedMethods = 2,
                                MethodScanResults = new List<MethodScanResult>()
                                {
                                    new MethodScanResult()
                                    {
                                        Sinks = 1,
                                        Hits = 1,
                                        MethodName = "ThisIsVulnerableMethod",
                                        MethodBody = "public void ThisIsVulnerableMethod(string arg)\r\n        {\r\n            string myString = arg;\r\n            int arg3;\r\n\r\n            string arg1 = CreateStringValue(1 < 2 ? myString : string.Empty);\r\n            string arg2 = CreateStringValue(\"\");\r\n            arg3 = 0;\r\n            arg3 = 1;\r\n\r\n            SinkMethodOne(arg1, arg2, arg3, new MyClass(string.Empty));\r\n        }",
                                        LineNumber = 10,
                                        LineCount = 11,
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
