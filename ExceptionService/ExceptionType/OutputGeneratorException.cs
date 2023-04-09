using System;

namespace ExceptionService.ExceptionType
{
    /// <summary>
    /// ExceptionService.ExceptionType <c>OutputGeneratorException</c> class.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    public class OutputGeneratorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="OutputGeneratorException"/> class with the message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public OutputGeneratorException(string message) : base(message) { }
    }
}