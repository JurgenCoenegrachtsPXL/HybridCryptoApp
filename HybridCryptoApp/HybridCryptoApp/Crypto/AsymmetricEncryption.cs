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
            CspParameters cspParameters = GetCSPParameters();

            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = true;
                return rsa.Decrypt(data, false);
            }
        }

        /// <summary>
        /// Get XML string representation of public key
        /// </summary>
        /// <returns>XML string representation of public key</returns>
        public static string PublicKeyAsXml()
        {
            CspParameters cspParameters = new CspParameters(1);
            cspParameters.KeyContainerName = containerName;
            cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParameters.ProviderName = "Microsoft Strong Cryptographic Provider";

            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = true;
                return rsa.ToXmlString(false);
            }
        }

        /// <summary>
        /// Create public key from an XML string
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>Public key RSA parameters</returns>
        public static RSAParameters PublicKeyFromXml(string xml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;

                rsa.FromXmlString(xml);

                return rsa.ExportParameters(false);
            }
        }

        /// <summary>
        /// Sign data with private RSA key
        /// </summary>
        /// <param name="data">Data to sign</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] data)
        {
            CspParameters cspParameters = GetCSPParameters();
            
            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = true;

                var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                rsaFormatter.SetHashAlgorithm("SHA512");
                return rsaFormatter.CreateSignature(data);
            }
        }

        /// <summary>
        /// Check given signature
        /// </summary>
        /// <param name="signature">Signature</param>
        /// <param name="publicKey">Public key of user who created signature</param>
        /// <param name="expectedResult">Expected answer of signature</param>
        /// <returns>True or false</returns>
        public static bool CheckSignature(byte[] signature, RSAParameters publicKey, byte[] expectedResult)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);

                var rsaFormatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaFormatter.SetHashAlgorithm("SHA512");
                return rsaFormatter.VerifySignature(expectedResult, signature);
            }
        }

        private static CspParameters GetCSPParameters()
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

            return cspParameters;
        }
    }
}
