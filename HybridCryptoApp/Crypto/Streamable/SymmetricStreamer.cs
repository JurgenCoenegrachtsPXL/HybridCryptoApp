﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace HybridCryptoApp.Crypto.Streamable
{
    public class SymmetricStreamer : IDisposable
    {
        private readonly AesCryptoServiceProvider aes;

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
            return new CryptoStream(inputStream, aes.CreateEncryptor(), cryptoStreamMode);
        }

        /// <summary>
        /// Start decrypting stream
        /// </summary>
        /// <param name="inputStream">Stream to decrypt</param>
        /// <param name="cryptoStreamMode">Streaming mode</param>
        /// <returns></returns>
        public CryptoStream DecryptStream(Stream inputStream, CryptoStreamMode cryptoStreamMode)
        {
            return new CryptoStream(inputStream, aes.CreateDecryptor(), cryptoStreamMode);
        }

        /// <summary>
        /// Close output stream and AesCryptoServiceProvider
        /// </summary>
        public void Dispose()
        {
            aes?.Dispose();
        }
    }
}
