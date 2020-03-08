using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace HybridCryptoApp.Crypto
{
    public class Hashing
    {
        /// <summary>
        /// Create Sha512 hash of data
        /// </summary>
        /// <param name="data">Data to hash</param>
        /// <param name="key">Aes key</param>
        /// <returns>Hash</returns>
        public static byte[] HmacSha(byte[] data, byte[] key)
        {
            using (var sha512 = HMACSHA512.Create())
            {
                sha512.Key = key;
                return sha512.ComputeHash(data);
            }
        }

        /// <summary>
        /// Create Sha512 hash of message
        /// </summary>
        /// <param name="message">Message to create hash of</param>
        /// <param name="key">Aes key</param>
        /// <returns>Hash</returns>
        public static byte[] HmacSha(string message, byte[] key)
        {
            Byte[] arrayBytes = Encoding.UTF8.GetBytes(message);
            return HmacSha(arrayBytes, key);
        }

        /// <summary>
        /// Compare two hashes, execution time will always be the same
        /// </summary>
        /// <param name="hash1"></param>
        /// <param name="hash2"></param>
        /// <returns></returns>
        public static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            bool result = hash1.Length == hash2.Length;
            int shortestHashLength = (hash1.Length < hash2.Length) ? hash1.Length : hash2.Length;

            for (int i = 0; i < shortestHashLength; i++)
            {
                result &= hash1[i] == hash2[i];
            }

            return result;
        }
    }
}
