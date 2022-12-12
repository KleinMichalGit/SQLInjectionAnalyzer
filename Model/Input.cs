using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Model <c>Input</c> class.
    /// 
    /// <para>
    /// Contains all the information specified via console at the beginning of the program.
    /// Namely, information about what should be analysed and which method should be used. Also,
    /// where the results should be generated, what parts of code should be skipped, where the config
    /// file is located, etc.
    /// </para>
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Mandatory.
        /// </summary>
        /// <value>
        /// The path of the C# source folder which should be analysed.
        /// </value>
        public string SourceFolderPath { get; set; }

        /// <summary>
        /// Mandatory.
        /// </summary>
        /// <value>
        /// The scope refers to the philosophy used during analysis, the type of compiled files, the time and memory
        /// requirements of the analysis, the accuracy of the received diagnostics, etc. All of these decisions are
        /// hidden under the simplicity of the <see cref="ScopeOfAnalysis"/>.
        /// </value>
        public ScopeOfAnalysis Scope { get; set; }

        /// <summary>
        /// Mandatory.
        /// </summary>
        /// <value>
        /// The path where the final results should be generated.
        /// </value>
        public string ExportPath { get; set; }
        
        /// <summary>
        /// Mandatory.
        /// </summary>
        /// <value>
        /// The path to the .json configuration file.
        /// </value>
        public string ConfigFilePath { get; set; }
        
        /// <summary>
        /// Optional.
        /// </summary>
        /// <value>
        /// The list of subpaths of files which should be omitted during analysis.
        /// </value>
        public List<string> ExcludeSubpaths { get; set; } = new List<string>();
        
        /// <summary>
        /// Optional.
        /// </summary>
        /// <value>
        ///   <c>true</c> if results should be writen on console in real-time, otherwise <c>false</c>.
        /// </value>
        public bool WriteOnConsole { get; set; }
        
        /// <summary>
        /// Optional.
        /// </summary>
        /// <value>
        ///   <c>true</c> if only tutorial should be writen on console without performing analysis, otherwise <c>false</c>.
        /// </value>
        public bool WriteTutorialAndExit { get; set; }
    }
}
