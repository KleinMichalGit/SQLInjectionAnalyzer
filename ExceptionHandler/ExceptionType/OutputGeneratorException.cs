using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHandler.ExceptionType
{
    [Serializable]
    public class OutputGeneratorException : Exception
    {
        public OutputGeneratorException() : base() { }
        public OutputGeneratorException(string message) : base(message) { }
        public OutputGeneratorException(string message, Exception innerException) : base(message, innerException) { }
    }
}
