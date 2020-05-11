using System.Security.Cryptography;

namespace HybridCryptoApp.Crypto
{
    public static class AsymmetricEncryption
    {
        private static string containerName;
        private static int keyLength = 4096;

        /// <summary>
        /// Public key of currently loaded RSA pair
        /// </summary>
        public static RSAParameters PublicKey {
            get
            {
                CspParameters cspParameters = new CspParameters { KeyContainerName = containerName };
                
                using (var rsa = new RSACryptoServiceProvider(keyLength, cspParameters))
                {
                    rsa.PersistKeyInCsp = true;
                    return rsa.ExportParameters(false);
                }
            }
        }

        /// <summary>
        /// Create a new public, private key pair and places it in the Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name to reference container by</param>
        /// <param name="length">Length of key in bits</param>
        /// <returns>Public key of the pair</returns>
        public static RSAParameters CreateNewKeyPair(string name, int length)
        {
            containerName = name;
            keyLength = length;

            CspParameters cspParameters = new CspParameters(1); // er moet 1 staan i/d constructor anders werkt het niet
            cspParameters.KeyContainerName = name;

            var rsa = new RSACryptoServiceProvider(keyLength, cspParameters);
            rsa.PersistKeyInCsp = true;
            return rsa.ExportParameters(false);
        }

        /// <summary>
        /// Select an existing key pair to be the active key pair
        /// </summary>
        /// <param name="name">Name of the key pair</param>
        /// <param name="length">Length of the key pair</param>
        /// <returns>Public key of the pair</returns>
        public static RSAParameters SelectKeyPair(string name, int length)
        {
            containerName = name;
            keyLength = length;

            CspParameters cspParameters = new CspParameters {KeyContainerName = name};

            var rsa = new RSACryptoServiceProvider(keyLength, cspParameters);
            rsa.PersistKeyInCsp = true;
            return rsa.ExportParameters(false);
        }

        /// <summary>
        /// Delete private key from Microsoft Strong Cryptographic Provider
        /// </summary>
        /// <param name="name">Name of container to dispose</param>
        public static void DeleteKey(string name)
        {
            CspParameters cspParameters = new CspParameters {KeyContainerName = name};

            var rsa = new RSACryptoServiceProvider(cspParameters);
            rsa.PersistKeyInCsp = false;
            rsa.Clear();

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
            byte[] encryptedBytes;
            using (var rsa = new RSACryptoServiceProvider(keyLength))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(publicKey);
                encryptedBytes = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }

            return encryptedBytes;
        }

        /// <summary>
        /// Decrypt data using private RSA key in the referenced container
        /// </summary>
        /// <param name="data">Encrypted data</param>
        /// <returns>Plaintext data</returns>
        public static byte[] Decrypt(byte[] data)
        {
            CspParameters cspParameters = new CspParameters { KeyContainerName = containerName };

            byte[] decryptedBytes;
            using (var rsa = new RSACryptoServiceProvider(keyLength, cspParameters))
            {
                decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            }

            return decryptedBytes;
        }

        /// <summary>
        /// Get XML string representation of public key
        /// </summary>
        /// <returns> XML string representation of public key</returns>
        public static string PublicKeyAsXml()
        {
            CspParameters cspParameters = new CspParameters {KeyContainerName = containerName};

            using (var rsa = new RSACryptoServiceProvider(keyLength, cspParameters))
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
        /// Sign hash with private RSA key
        /// </summary>
        /// <param name="hash">Data to sign</param>
        /// <returns>Signature</returns>
        public static byte[] Sign(byte[] hash)
        {
            CspParameters cspParameters = new CspParameters {KeyContainerName = containerName};
            byte[] signBytes;

            using (var rsa = new RSACryptoServiceProvider(keyLength, cspParameters))
            {
                rsa.PersistKeyInCsp = true;
                
                var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                rsaFormatter.SetHashAlgorithm("SHA512");

                signBytes = rsaFormatter.CreateSignature(hash);
            }

            return signBytes;
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
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(publicKey);
                
                var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA512");

                return rsaDeformatter.VerifySignature(expectedResult, signature);
            }
        }

        /// <summary>
        /// Create CSP parameters to load current asymmetric key
        /// </summary>
        /// <returns></returns>
        private static CspParameters GetCSPParameters()
        {
            // container needs to be specified
            if (containerName == null)
            {
                throw new CryptoException("No RSA private key loaded.");
            }

            return new CspParameters
            {
                KeyContainerName = containerName,
            };
        }
    }
}
