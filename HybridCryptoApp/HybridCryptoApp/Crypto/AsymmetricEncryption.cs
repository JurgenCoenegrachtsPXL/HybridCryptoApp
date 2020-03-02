using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto
{
    public static class AsymmetricEncryption
    {
        private static string containerName = null;

        /// <summary>
        /// Create a new public, private key pair in Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name to reference container by</param>
        /// <param name="keyLength">length of key in bits</param>
        /// <returns>Public key of the pair</returns>
        public static byte[] CreateNewKeyPair(string name, int keyLength)
        {
            containerName = name;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete private key from Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name of container to dispose</param>
        public static void DisposeKey(string name)
        {
            containerName = null;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypt data using RSA
        /// </summary>
        /// <param name="data">Plaintext data</param>
        /// <param name="publicKey">Public key of the receiver</param>
        /// <returns>Data encrypted with public key</returns>
        public static byte[] Encrypt(byte[] data, byte[] publicKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt data using private RSA key in the referenced container
        /// </summary>
        /// <param name="data">Encrypted data</param>
        /// <returns>Plaintext data</returns>
        public static byte[] Decrypt(byte[] data)
        {
            // container needs to be specified
            if (containerName == null)
            {
                throw new CryptoException("No RSA private key loaded.");
            }

            // TODO: use CSP
            throw new NotImplementedException();
        }
    }
}
