namespace Model
{
    /// <summary>
    /// Model <c>ScopeOfAnalysis</c> enumeration.
    /// <para>
    /// The scope of the analysis is set via input at the beginning of the
    /// program. The scope refers to the philosophy used during analysis, the
    /// type of compiled files, the time and memory requirements of the
    /// analysis, the accuracy of the received diagnostics, etc. All of these
    /// decisions are hidden under the simplicity of the
    /// <see cref="ScopeOfAnalysis"/>.
    /// </para>
    /// </summary>
    public enum ScopeOfAnalysis
    {
        /// <summary>
        /// Reads C# (*.cs) files separately and investigates Syntax Trees
        /// parsed from the separate C# files, without compiling .csproj files,
        /// without performing interprocedural analysis, able to decide trivial
        /// conditional statements (very fast but very inacurate).
        /// </summary>
        OneMethodSyntaxTree,

        /// <summary>
        /// Compiles *.csproj files, without performing interprocedural
        /// analysis. Uses the same rules as OneMethodSyntaxTree, therefore
        /// provides the same results. This <see cref="ScopeOfAnalysis"/> serves
        /// only to investigate how much time is needed for compilation of all
        /// .csproj files. Able to decide trivial conditional statements.
        /// </summary>
        OneMethodCSProj,

        /// <summary>
        /// Compiles all C# project (*.csproj) files, performs n-level
        /// interprocedural analysis (where number n is defined in config.json
        /// file) for each project separately, able to decide trivial
        /// conditional statements.
        /// </summary>
        InterproceduralCSProj,

        /// <summary>
        /// Opens all C# solution (*.sln) files, performs n-level
        /// interprocedural analysis (where number n is defined in config.json
        /// file) for each solution separately, able to decide trivial
        /// conditional statements.
        /// </summary>
        InterproceduralSolution,
        
        TestScope,
    }
}