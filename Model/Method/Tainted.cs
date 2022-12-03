namespace Model.Method
{
    /// <summary>
    /// 
    /// </summary>
    public class Tainted
    {
        //for each tainted parameter and argument remember how many times it is tainted
        /// <summary>
        /// The tainted method parameters
        /// </summary>
        public int[] TaintedMethodParameters;
        /// <summary>
        /// The tainted invocation arguments
        /// </summary>
        public int[] TaintedInvocationArguments;
    }
}
