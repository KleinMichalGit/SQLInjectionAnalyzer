﻿namespace OutputService.RazorOutput
{
    /// <summary>
    /// OutputService.RazorOutput <c>ReportInterproceduralCSProj</c> class.
    /// 
    /// <para>
    /// Template for InterproceduralCSProj <see cref="ScopeOfAnalysis"/>
    /// </para>
    /// </summary>
    public class ReportInterproceduralCSProj
    {
        /// <summary>
        /// Template of the report
        /// </summary>
        public static string report = "<!doctype html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"utf-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=no\">\r\n    <script src=\"https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js\"></script>\r\n    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-GLhlTQ8iRABdZLl6O3oVMWSktQOp6b7In1Zl3/Jr59b6EGGoI1aFkw7cmDA6j6gD\" crossorigin=\"anonymous\">\r\n    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/js/bootstrap.bundle.min.js\" integrity=\"sha384-w76AqPfDkMBDXo30jS1Sgez6pr3x5MlQ1ZAGC+nuZB+EYdgRZgiwxhTBTkF7CXvN\" crossorigin=\"anonymous\"></script>\r\n    <script src=\"https://cdn.jsdelivr.net/gh/google/code-prettify@master/loader/run_prettify.js\"></script>\r\n    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>\r\n</head>\r\n<body>\r\n    <nav class=\"navbar fixed-top bg-body-tertiary \">\r\n        <div class=\"container-fluid\">\r\n            <a class=\"navbar-brand\" href=\"#\">SQL Injection Analyzer (InterproceduralCSProj scope of analysis)</a>\r\n        </div>\r\n    </nav>\r\n    <div class=\"container\" style=\"margin-top: 70px;\">\r\n        <h2>Report statistics</h2>\r\n        <table class=\"table table-dark table-hover\">\r\n            <tbody>\r\n                <tr>\r\n                    <th scope=\"row\">Analysis start time</th>\r\n                    <td>@Model.Diagnostics.DiagnosticsStartTime</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Analysis end time</th>\r\n                    <td>@Model.Diagnostics.DiagnosticsEndTime</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Analysis total time</th>\r\n                    <td>@Model.Diagnostics.DiagnosticsTotalTime</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of all .csproj files in scanned directory</th>\r\n                    <td>@Model.NumberOfAllCSProjFiles</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of compiled .csproj files</th>\r\n                    <td>@Model.NumberOfScannedCSProjFiles</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of skipped .csproj files</th>\r\n                    <td>@Model.NumberOfSkippedCSProjFiles</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of all C# files (.cs) referred to in all compiled .csproj files</th>\r\n                    <td>@Model.NumberOfAllCSFiles</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of scanned methods</th>\r\n                    <td>@Model.NumberOfScannedMethods</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of skipped methods</th>\r\n                    <td>@Model.NumberOfSkippedMethods</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">Number of all sink invocations</th>\r\n                    <td>@Model.NumberOfAllSinks</td>\r\n                </tr>\r\n                <tr>\r\n                    <th scope=\"row\">\r\n                        @if (Model.NumberOfVulnerableMethods > 0)\r\n                        {<span class=\"badge text-bg-danger\">Vulnerabilities detected: </span> }\r\n                        else\r\n                        { <span class=\"badge text-bg-success\">OK (no vulnerabilities detected)</span>} Number of vulnerable methods\r\n                    </th>\r\n                    <td>@Model.NumberOfVulnerableMethods</td>\r\n                </tr>\r\n            </tbody>\r\n        </table>\r\n\r\n        <h2>Horizontal bar chart for visualization of vulnerable and safe methods</h2>\r\n        <div style=\"width: 800px; height: 200px;\">\r\n            <canvas id=\"myChart\"></canvas>\r\n            <script>\r\n            const ctx = document.getElementById('myChart').getContext('2d');\r\n            const myChart = new Chart(ctx, {\r\n                type: 'bar',\r\n                data: {\r\n                    labels: [\r\n                        'Safe Methods',\r\n                        'Vulnerable Methods'\r\n                    ],\r\n                    datasets: [{\r\n                        axis: 'y',\r\n                        label: 'Horizontal bar chart for visualization of vulnerable and safe methods',\r\n                        data: [\r\n                            @Model.NumberOfSkippedMethods + @Model.NumberOfScannedMethods - @Model.NumberOfVulnerableMethods,\r\n                            @Model.NumberOfVulnerableMethods\r\n                        ],\r\n                        fill: false,\r\n                        backgroundColor: [\r\n                            'rgba(0, 255, 0, 0.2)',\r\n                            'rgba(255, 0, 0, 0.2)'\r\n                        ],\r\n                        borderColor: [\r\n                            'rgb(0, 255, 0)',\r\n                            'rgb(255, 0, 0)'\r\n                        ],\r\n                        borderWidth: 1,\r\n\r\n                    }]\r\n                },\r\n                options: {\r\n                    indexAxis: 'y',\r\n                }\r\n            });\r\n            </script>\r\n        </div>\r\n\r\n        @if (Model.NumberOfVulnerableMethods > 0)\r\n        {\r\n            <h3>Vulnerable Methods Detailed Overview</h3>\r\n            @foreach (var csProjectScanResult in Model.Diagnostics.CSProjectScanResults)\r\n            {\r\n                @if (Model.RemoteDataExtractor.GetNumberOfVulnerableMethodsInCSProj(csProjectScanResult) > 0)\r\n                {\r\n                    <div class=\"card text-white bg-dark mb-3\">\r\n                        <div class=\"card-header\"><h5>.csproj scan result</h5></div>\r\n                        <div class=\"card-body\">\r\n                            <h5 class=\"card-title\">Path: @csProjectScanResult.Path</h5>\r\n\r\n                            <p class=\"card-text\">.csproj scan start time: @csProjectScanResult.CSProjectScanResultStartTime</p>\r\n                            <p class=\"card-text\">.csproj scan end time: @csProjectScanResult.CSProjectScanResultEndTime</p>\r\n                            <p class=\"card-text\">.csproj scan total time: @csProjectScanResult.CSProjectScanResultTotalTime</p>\r\n                        </div>\r\n                    </div>\r\n\r\n                    @foreach (var syntaxTreeScanResult in csProjectScanResult.SyntaxTreeScanResults)\r\n                    {\r\n                        @if (Model.RemoteDataExtractor.GetNumberOfVulnerableMethodsInFile(syntaxTreeScanResult) > 0)\r\n                        {\r\n                            <div class=\"card bg-info mb-3\">\r\n                                <div class=\"card-header\">\r\n                                    <h5>C# file scan report</h5>\r\n                                </div>\r\n                                <div class=\"card-body\">\r\n                                    <h5>file path: @syntaxTreeScanResult.Path</h5>\r\n                                    <p>file scan start time: @syntaxTreeScanResult.SyntaxTreeScanResultStartTime</p>\r\n                                    <p>file scan end time: @syntaxTreeScanResult.SyntaxTreeScanResultEndTime</p>\r\n                                    <p>file scan total time: @syntaxTreeScanResult.SyntaxTreeScanResultTotalTime</p>\r\n                                </div>\r\n                            </div>\r\n                        }\r\n\r\n                        @foreach (var methodScanResult in syntaxTreeScanResult.MethodScanResults)\r\n                        {\r\n                            @if (methodScanResult.Hits > 0)\r\n                            {\r\n                                <div class=\"card bg-light mb-3\">\r\n                                    <div class=\"card-header\">\r\n                                        <h5>Method name: @methodScanResult.MethodName</h5>\r\n                                        <h5>Line number: @methodScanResult.LineNumber</h5>\r\n                                        <span class=\"badge text-bg-info\">Sinks: @methodScanResult.Sinks</span>\r\n                                        <span class=\"badge text-bg-danger\">Hits: @methodScanResult.Hits</span>\r\n                                        @foreach (var sourceLabel in methodScanResult.SourceAreasLabels)\r\n                                        {\r\n                                            <span class=\"badge text-bg-warning\">@sourceLabel</span>\r\n                                        }\r\n                                    </div>\r\n                                    <div class=\"card-body\">\r\n                                        <ul class=\"list-group list-group-flush bg-light\">\r\n                                            <li class=\"list-group-item bg-light\">\r\n                                                <h6>n-level BFS Inteprocedural-callers tree</h6>\r\n                                                <pre class=\"prettyprint bg-white\">@methodScanResult.CallersTree</pre>\r\n                                            </li>\r\n                                            <li class=\"list-group-item bg-light\">\r\n                                                <h6>Method body</h6>\r\n                                                <pre class=\"prettyprint bg-white\">@methodScanResult.MethodBody</pre>\r\n                                            </li>\r\n                                            <li class=\"list-group-item bg-light\">\r\n                                                <h6>Evidence</h6>\r\n                                                <pre class=\"prettyprint bg-white\">@methodScanResult.Evidence</pre>\r\n                                            </li>\r\n                                        </ul>\r\n                                    </div>\r\n                                </div>\r\n                            }\r\n                        }\r\n                    }\r\n                }\r\n            }\r\n        }\r\n        else\r\n        {\r\n            <h3>No vulnerabilities were detected. Injection analysis test passed successfully.</h3>\r\n        }\r\n    </div>\r\n</body>\r\n</html>\r\n";
    }
}
