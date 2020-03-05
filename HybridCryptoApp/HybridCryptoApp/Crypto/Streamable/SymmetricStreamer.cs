using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto.Streamable
{
    public class SymmetricStreamer : IDisposable
    {
        private CryptoStream outputStream = null;
        private AesCryptoServiceProvider aes = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Secret key</param>
        /// <param name="iv">Initialization Vector</param>
        public SymmetricStreamer(byte[] key, byte[] iv)
        {
            aes = new AesCryptoServiceProvider { Key = key, IV = iv };
        }

        /// <summary>
        /// Start encrypting stream
        /// </summary>
        /// <param name="inputStream">Stream to encrypt</param>
        /// <param name="cryptoStreamMode">Streaming mode</param>
        /// <returns></returns>
        public CryptoStream EncryptStream(Stream inputStream, CryptoStreamMode cryptoStreamMode)
        {
            outputStream = new CryptoStream(inputStream, aes.CreateEncryptor(), cryptoStreamMode);
            return outputStream;
        }

        /// <summary>
        /// Start decrypting stream
        /// </summary>
        /// <param name="inputStream">Stream to decrypt</param>
        /// <param name="cryptoStreamMode">Streaming mode</param>
        /// <returns></returns>
        public CryptoStream DecryptStream(Stream inputStream, CryptoStreamMode cryptoStreamMode)
        {
            outputStream = new CryptoStream(inputStream, aes.CreateDecryptor(), cryptoStreamMode);
            return outputStream;
        }

        /// <summary>
        /// Close output stream and AesCryptoServiceProvider
        /// </summary>
        public void Dispose()
        {
            outputStream?.Dispose();
            aes?.Dispose();
        }
    }
}
