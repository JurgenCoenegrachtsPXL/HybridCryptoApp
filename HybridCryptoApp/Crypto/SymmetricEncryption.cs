using System;
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
        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="data">Data which needs to be encrypted</param>
        /// <param name="key">Secret key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv;

                using (var memoryStream = new MemoryStream())
                {
                    var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypt data with AES
        /// </summary>
        /// <param name="data">Data which needs to be decrypted</param>
        /// <param name="key">Secret key</param>
        /// <param name="iv">Initialization Vector</param>
        /// <returns>Plaintext data</returns>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
            using (var aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data, 0, data.Length);
                    }

                    return memoryStream.ToArray();
                }
            }
        }
    }
}
