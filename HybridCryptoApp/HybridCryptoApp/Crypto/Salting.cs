using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;

namespace HybridCryptoApp.Crypto
{
    public class Salting
    {
        public byte[] CreateSalt(string password, byte[] salt)
        {
            var argon = Argon2.Hash(password);
            return null;
        }
    }
}
