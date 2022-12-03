using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public class Input
    {
        //mandatory
        /// <summary>
        /// Gets or sets the source folder path.
        /// </summary>
        /// <value>
        /// The source folder path.
        /// </value>
        public string SourceFolderPath { get; set; }
        //mandatory
        /// <summary>
        /// Gets or sets the scope.
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        public ScopeOfAnalysis Scope { get; set; }
        //mandatory
        /// <summary>
        /// Gets or sets the export path.
        /// </summary>
        /// <value>
        /// The export path.
        /// </value>
        public string ExportPath { get; set; }
        //mandatory
        /// <summary>
        /// Gets or sets the configuration file path.
        /// </summary>
        /// <value>
        /// The configuration file path.
        /// </value>
        public string ConfigFilePath { get; set; }
        //optional
        /// <summary>
        /// Gets or sets the exclude subpaths.
        /// </summary>
        /// <value>
        /// The exclude subpaths.
        /// </value>
        public List<string> ExcludeSubpaths { get; set; } = new List<string>();
        //optional
        /// <summary>
        /// Gets or sets a value indicating whether [write on console].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [write on console]; otherwise, <c>false</c>.
        /// </value>
        public bool WriteOnConsole { get; set; }
        //optional
        /// <summary>
        /// Gets or sets a value indicating whether [write tutorial and exit].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [write tutorial and exit]; otherwise, <c>false</c>.
        /// </value>
        public bool WriteTutorialAndExit { get; set; }
    }
}
