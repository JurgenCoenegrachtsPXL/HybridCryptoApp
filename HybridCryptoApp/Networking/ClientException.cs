using System;
using System.Runtime.Serialization;

namespace HybridCryptoApp.Networking
{
    public class ClientException : Exception
    {
        /// <inheritdoc />
        public ClientException()
        {
        }

        /// <inheritdoc />
        public ClientException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ClientException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected ClientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}