using System;

namespace HybridCryptoApp.Crypto
{
    public class CryptoException : Exception
    {
        /// <inheritdoc />
        public CryptoException()
        {
        }

        /// <inheritdoc />
        public CryptoException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public CryptoException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
