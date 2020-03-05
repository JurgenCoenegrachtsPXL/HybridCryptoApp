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
        //zowel files als berichten sturen

        public static byte[] Sha(byte[] data)
        {
            using (var sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(data);
            }
        }

        public static byte[] HmacSha(byte[] data, byte[] key)
        {
            using (var sha512 = HMACSHA512.Create())
            {
                sha512.Key = key;
                return sha512.ComputeHash(data);
            }
        }

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

        /// <summary>
        /// Create HMAC from stream
        /// </summary>
        /// <param name="inputStream">Stream to create HMAC from</param>
        /// <param name="key">Secret AES key to use</param>
        /// <returns></returns>
        public static Stream HmacShaStream(Stream inputStream, byte[] key)
        {
            using (var sha = new HMACSHA512())
            {
                sha.Key = key;
                using (var stream = new MemoryStream())
                {
                    //TODO 
                    throw new NotImplementedException();
                }
            }
        }
    }
}
