using System;

namespace ExceptionService.ExceptionType
{
    /// <summary>
    /// ExceptionService.ExceptionType <c>AnalysisException</c> class.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    public class AnalysisException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisException"/>
        /// class with the message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AnalysisException(string message) : base(message) { }
    }
}