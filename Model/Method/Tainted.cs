namespace Model.Method
{
    public class Tainted
    {

        /// <summary>
        /// for each tainted parameter and argument remember how many times it is tainted
        /// </summary>
        public int[] TaintedMethodParameters;

        public int[] TaintedInvocationArguments;
    }
}
