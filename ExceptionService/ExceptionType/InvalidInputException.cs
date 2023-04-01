using System;

namespace ExceptionService.ExceptionType
{
    /// <summary>
    /// ExceptionService.ExceptionType <c>InvalidInputException</c> class.
    /// </summary>
    /// <seealso cref="System.Exception"/>
    [Serializable]
    public class InvalidInputException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="InvalidInputException"/> class.
        /// </summary>
        public InvalidInputException() : base() { }
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="InvalidInputException"/> class with the message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidInputException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="InvalidInputException"/> class with the message and
        /// innerException <see cref="Exception"/>.
        /// </summary>
        /// <param name="message">The error message that explains the reason for
        ///     the exception.</param>
        /// <param name="innerException">The exception that is the cause of the
        ///     current exception, or a null reference
        ///     (<see langword="Nothing"/> in Visual Basic) if no inner
        ///     exception is specified.</param>
        public InvalidInputException(string message, Exception innerException) : base(message, innerException) { }
    }
}
