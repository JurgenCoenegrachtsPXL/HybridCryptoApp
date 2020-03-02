using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
