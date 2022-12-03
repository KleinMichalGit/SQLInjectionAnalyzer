using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionHandler.ExceptionType
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class AnalysisException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisException"/> class.
        /// </summary>
        public AnalysisException() : base() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AnalysisException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public AnalysisException(string message, Exception innerException) : base(message, innerException) { }
    }
}
