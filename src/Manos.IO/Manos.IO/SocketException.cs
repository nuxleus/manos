using System;
using System.Runtime.Serialization;

namespace Manos.IO
{
    [Serializable]
    public class SocketException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:SocketException"/> class
        /// </summary>
        public SocketException(SocketError code)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SocketException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="code">Error code of the operation that failed.</param>
        public SocketException(string message, SocketError code)
            : base(message)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SocketException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
        /// <param name="code">Error code of the operation that failed.</param>
        /// <param name="inner">The exception that is the cause of the current exception. </param>
        public SocketException(string message, SocketError code, Exception inner)
            : base(message, inner)
        {
            ErrorCode = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SocketException"/> class
        /// </summary>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <param name="info">The object that holds the serialized object data.</param>
        protected SocketException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SocketError ErrorCode { get; private set; }
    }
}