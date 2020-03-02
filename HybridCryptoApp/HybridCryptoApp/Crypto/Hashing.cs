using System;
using System.Collections.Generic;
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

        public static byte[] Argon2(byte[] data, byte[] salt, byte[] key)
        {
            throw new NotImplementedException();
        }

        public static byte[] Argon2(string message, byte[] salt, byte[] key)
        {
            throw new NotImplementedException();
        }

    }
}
