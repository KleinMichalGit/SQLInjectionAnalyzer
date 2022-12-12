namespace Model
{
    /// <summary>
    /// Model <c>ScopeOfAnalysis</c> enumeration.
    /// 
    /// <para>
    /// The scope of the analysis is set via input at the beginning of the program. The scope 
    /// refers to the philosophy used during analysis, the type of compiled files, the time and memory
    /// requirements of the analysis, the accuracy of the received diagnostics, etc. All of these decisions are
    /// hidden under the simplicity of the <see cref="ScopeOfAnalysis"/>.
    /// </para>
    /// </summary>
    public enum ScopeOfAnalysis
    {
        /// <summary>
        /// Reads *.cs files separately, without compiling .csproj files, without performing interprocedural analysis,
        /// every block of code is considered as reachable (very fast but very inacurate.).
        /// </summary>
        Simple,
        /// <summary>
        /// Compiles *.csproj files, without performing interprocedural analysis.
        /// </summary>
        OneMethod,
        /// <summary>
        /// Compiles *.csproj files, performs n-level interprocedural analysis,
        /// every block of code is considered as reachable.
        /// </summary>
        Interprocedural,
        /// <summary>
        /// Compiles *.csproj files, performs n-level interprocedural analysis,
        /// able to decide trivial problems when solving reachability problems.
        /// </summary>
        InterproceduralReachability
    }
}
