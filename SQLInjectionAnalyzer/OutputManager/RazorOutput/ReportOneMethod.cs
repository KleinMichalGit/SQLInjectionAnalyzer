﻿namespace SQLInjectionAnalyzer.OutputManager.RazorOutput
{
    /// <summary>
    /// SQLInjectionAnalyzer.OutputManager.RazorOutput <c>ReportOneMethod</c> class.
    /// 
    /// <para>
    /// Template for OneMethod scope of analysis.
    /// </para>
    /// </summary>
    public class ReportOneMethod
    {
        /// <summary>
        /// Template of the report
        /// </summary>
        public static string report = "<!doctype html>\r\n<html lang=\"en\">\r\n<head>\r\n    <meta charset=\"utf-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=no\">\r\n    <script src=\"https://cdn.jsdelivr.net/npm/jquery@3.6.0/dist/jquery.min.js\"></script>\r\n    <link href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.6.0/dist/css/bootstrap.min.css\" rel=\"stylesheet\">\r\n    <script src=\"https://cdn.jsdelivr.net/npm/bootstrap@4.6.0/dist/js/bootstrap.bundle.min.js\"></script>\r\n    <script src=\"https://cdn.jsdelivr.net/gh/google/code-prettify@master/loader/run_prettify.js\"></script>\r\n    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>\r\n</head>\r\n<body>\r\n\r\n    <div class=\"container\">\r\n     <h1>SQLi Analysis Report (One method scope)</h1>\r\n     <div class=\"row\">\r\n        <div class=\"col\">\r\n            <table class=\"table-sm\">\r\n                <tbody>\r\n                    <tr>\r\n                        <th scope=\"row\">Analysis start time</th>\r\n                        <td>@Model.Diagnostics.DiagnosticsStartTime</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Analysis end time</th>\r\n                        <td>@Model.Diagnostics.DiagnosticsEndTime</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Analysis total time</th>\r\n                        <td>@Model.Diagnostics.DiagnosticsTotalTime</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of all .csproj files in scanned directory</th>\r\n                        <td>@Model.NumberOfAllCSProjFiles</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of scanned .csproj files</th>\r\n                        <td>@Model.NumberOfScannedCSProjFiles</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of skipped .csproj files</th>\r\n                        <td>@Model.NumberOfSkippedCSProjFiles</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of all .cs files in all scanned .csproj files</th>\r\n                        <td>@Model.NumberOfAllCSFiles</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of scanned methods</th>\r\n                        <td>@Model.NumberOfScannedMethods</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of skipped methods</th>\r\n                        <td>@Model.NumberOfSkippedMethods</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of all sink invocations</th>\r\n                        <td>@Model.NumberOfAllSinks</td>\r\n                    </tr>\r\n                    <tr>\r\n                        <th scope=\"row\">Number of vulnerable methods</th>\r\n                        <td>@Model.NumberOfVulnerableMethods</td>\r\n                    </tr>\r\n                </tbody>\r\n            </table>\r\n        </div>\r\n\r\n       \r\n       </div>\r\n        \r\n        @if (Model.NumberOfVulnerableMethods > 0)\r\n        {\r\n            <h3>Vulnerable Methods Detailed Overview</h3>\r\n            @foreach(var csProjectScanResult in Model.Diagnostics.CSProjectScanResults)\r\n            {\r\n                @if (Model.RemoteDataExtractor.GetNumberOfVulnerableMethodsInCSProj(csProjectScanResult) > 0)\r\n                {\r\n                    <div class=\"card text-white bg-dark mb-3\">\r\n                        <div class=\"card-header\">.csproj scan result</div>\r\n                        <div class=\"card-body\">\r\n                            <h5 class=\"card-title\">Path: @csProjectScanResult.Path</h5>\r\n                            \r\n                            <p class=\"card-text\">.csproj scan start time: @csProjectScanResult.CSProjectScanResultStartTime</p>\r\n                            <p class=\"card-text\">.csproj scan end time: @csProjectScanResult.CSProjectScanResultEndTime</p>\r\n                            <p class=\"card-text\">.csproj scan total time: @csProjectScanResult.CSProjectScanResultTotalTime</p>\r\n                        </div>\r\n                    </div>\r\n                \r\n                    @foreach(var syntaxTreeScanResult in csProjectScanResult.SyntaxTreeScanResults)\r\n                    {\r\n                        @if (Model.RemoteDataExtractor.GetNumberOfVulnerableMethodsInFile(syntaxTreeScanResult) > 0)\r\n                        {\r\n                            <h5>file path: @syntaxTreeScanResult.Path</h5>\r\n                            <p>file scan start time: @syntaxTreeScanResult.SyntaxTreeScanResultStartTime</p>   \r\n                            <p>file scan end time: @syntaxTreeScanResult.SyntaxTreeScanResultEndTime</p>   \r\n                            <p>file scan total time: @syntaxTreeScanResult.SyntaxTreeScanResultTotalTime</p>   \r\n                        }\r\n                        \r\n                        @foreach(var methodScanResult in syntaxTreeScanResult.MethodScanResults)\r\n                        {\r\n                            @if (methodScanResult.Hits > 0)\r\n                            {\r\n                             <div class=\"card bg-light mb-3\">\r\n                                <div class=\"card-header\">\r\n                                    <h5>@methodScanResult.MethodName</h5>\r\n                                    <span class=\"badge badge-info\">Sinks: @methodScanResult.Sinks</span>\r\n                                    <span class=\"badge badge-danger\">Hits: @methodScanResult.Hits</span>\r\n                                </div>\r\n                                <div class=\"card-body\">\r\n                                    <ul class=\"list-group list-group-flush bg-light\">\r\n                                    <li class=\"list-group-item bg-light\">\r\n                                        <h6>Method body</h6>\r\n                                        <pre class=\"prettyprint bg-white\">@methodScanResult.MethodBody</pre>\r\n                                    </li>\r\n                                    <li class=\"list-group-item bg-light\">\r\n                                        <h6>Evidence</h6>\r\n                                        <pre class=\"prettyprint bg-white\">@methodScanResult.Evidence</pre>\r\n                                    </li>\r\n                                </ul>\r\n                                </div>\r\n                             </div>   \r\n                            }\r\n                        }\r\n                    }\r\n                }\r\n            }\r\n        } else\r\n        {\r\n            <h3>No vulnerabilities were detected. Injection analysis test passed successfully.</h3>\r\n        }\r\n    </div>    \r\n</body>\r\n</html>";
    }
}
