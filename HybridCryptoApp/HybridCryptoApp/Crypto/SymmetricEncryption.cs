﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto
{
    public static class SymmetricEncryption
    {
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv; // initialization vector

                using (var memoryStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv; // initialization vector

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Encrypt a stream with AES
        /// </summary>
        /// <param name="inputStream">Stream of data which needs to be encrypted</param>
        /// <param name="key">Secret key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns>Input encrypted with AES</returns>
        public static Stream EncryptFileStream(Stream inputStream, byte[] key, byte[] iv)
        {
            // TODO: clean up this mess

            //FileStream inputStream = null;

            try
            {
                //inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);

                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    CryptoStream outputStream = new CryptoStream(inputStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    return outputStream;
                }
            }
            catch (IOException)
            {
                return null;
            }
            finally
            {
                //inputStream?.Close();
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt a stream with AES
        /// </summary>
        /// <param name="inputStream">Stream of encrypted data</param>
        /// <param name="key">Secret key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns>Unencrypted data</returns>
        public static Stream DecryptFileStream(Stream inputStream, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                aes.IV = iv;

                CryptoStream outputStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Write);
                return outputStream;
            }
        }
    }
}
