using System;

namespace ExceptionService.ExceptionType
{
    /// <summary>
    /// ExceptionService.ExceptionType <c>InvalidInputException</c> class.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    public class InvalidInputException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="InvalidInputException"/> class with the message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidInputException(string message) : base(message) { }
    }
}