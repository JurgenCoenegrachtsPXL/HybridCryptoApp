using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto
{
    public static class AsymmetricEncryption
    {
        private static string containerName = null;

        /// <summary>
        /// Create a new public, private key pair and places it in the Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name to reference container by</param>
        /// <param name="keyLength">Length of key in bits</param>
        /// <returns>Public key of the pair</returns>
        public static RSAParameters CreateNewKeyPair(string name, int keyLength)
        {
            containerName = name;

            CspParameters cspParameters = new CspParameters(1); // er moet 1 staan i/d constructor anders werkt het niet
            cspParameters.KeyContainerName = name;
            cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParameters.ProviderName = "Microsoft Strong Cryptographic Provider";

            using (var rsa = new RSACryptoServiceProvider(keyLength, cspParameters))
            {
                rsa.PersistKeyInCsp = true;
                return rsa.ExportParameters(false);
            }
        }

        /// <summary>
        /// Delete private key from Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name of container to dispose</param>
        public static void DisposeKey(string name)
        {
            CspParameters cspParameters = new CspParameters(1); // er moet 1 staan i/d constructor anders werkt het niet
            cspParameters.KeyContainerName = name;
            cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParameters.ProviderName = "Microsoft Strong Cryptographic Provider";

            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = false;
                rsa.Clear();
            }

            containerName = null;
        }

        /// <summary>
        /// Encrypt data using RSA
        /// </summary>
        /// <param name="data">Plaintext data</param>
        /// <param name="publicKey">Public key of the receiver</param>
        /// <returns>Data encrypted with public key</returns>
        public static byte[] Encrypt(byte[] data, RSAParameters publicKey)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                return rsa.Encrypt(data,false);
            }
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

            CspParameters cspParameters = new CspParameters(1); // er moet 1 staan i/d constructor anders werkt het niet
            cspParameters.KeyContainerName = containerName;
            cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParameters.ProviderName = "Microsoft Strong Cryptographic Provider";

            
            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = true;
                return rsa.Decrypt(data, false);
            }
        }

        //TODO : STREAMS

        /// <summary>
        /// Encrypt a stream with RSA
        /// </summary>
        /// <param name="inputStream">Stream of data to encrypt</param>
        /// <param name="publicKey">Public key of the receiver</param>
        /// <returns>Stream of encrypted data</returns>
        public static Stream EncryptStream(Stream inputStream, byte[] publicKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt a stream with RSA
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        public static Stream DecryptStream(Stream inputStream)
        {
            throw new NotImplementedException();
        }

        public static string PublicKeyAsXml()
        {
            throw new NotImplementedException();
        }

        public static RSAParameters PublicKeyFromXml(string xml)
        {
            throw new NotImplementedException();
        }
    }
}
