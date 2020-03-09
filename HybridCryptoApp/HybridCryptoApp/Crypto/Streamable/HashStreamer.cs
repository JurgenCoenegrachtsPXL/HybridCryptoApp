using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto.Streamable
{
    public class HashStreamer : IDisposable
    {
        private HMACSHA512 hasher;
        private CryptoStream hashStream;
        public byte[] Hash => hasher.Hash;

        public HashStreamer(byte[] key)
        {
            hasher = new HMACSHA512(key);
        }

        /// <summary>
        /// Create HMAC from stream
        /// </summary>
        /// <param name="inputStream">Stream to create HMAC from</param>
        /// <param name="key">Secret AES key to use</param>
        /// <param name="cryptoStreamMode">Read or Write stream</param>
        /// <returns></returns>
        public Stream HmacShaStream(Stream inputStream, byte[] key, CryptoStreamMode cryptoStreamMode)
        {
            hasher.Key = key;

            hashStream = new CryptoStream(inputStream, hasher, cryptoStreamMode);
            return hashStream;
        }

        /// <summary>
        /// Close stream and hasher
        /// </summary>
        public void Dispose()
        {
            //hashStream?.Dispose();
            //hasher?.Dispose();
        }
    }
}
