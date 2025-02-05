﻿using System.Security.Cryptography;

namespace HybridCryptoApp.Crypto
{
    public static class Random
    {
        /// <summary>
        /// Create an array of random bytes
        /// </summary>
        /// <param name="length">Amount of bytes to generate</param>
        /// <returns>An array of random bytes</returns>
        public static byte[] GetNumbers(int length)
        {
            using (var random = new RNGCryptoServiceProvider())
            {
                byte[] array = new byte[length];

                random.GetBytes(array);

                return array;
            }
        }
    }
}
