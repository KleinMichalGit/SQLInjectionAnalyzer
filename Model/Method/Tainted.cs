using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Method
{
    public class Tainted
    {
        //for each tainted parameter and argument remember how many times it is tainted
        public int[] TaintedMethodParameters;
        public int[] TaintedInvocationArguments;
    }
}
