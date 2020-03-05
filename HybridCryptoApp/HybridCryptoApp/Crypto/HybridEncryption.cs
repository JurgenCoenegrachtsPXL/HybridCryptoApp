using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto
{
    public class HybridEncryption
    {
        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="type">Type of data</param>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="publicKey">Public key of receiver</param>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket Encrypt(DataType type, byte[] data, RSAParameters publicKey)
        {
            // TODO: create EAS session key
            // TODO: create EAS IV
            // TODO: encrypt data with EAS
            // TODO: generate hash of encrypted data
            // TODO: encrypt EAS session key with RSA
            // TODO: put all info into new EncryptedPacket
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt an encrypted packet of data
        /// </summary>
        /// <param name="encryptedPacket"></param>
        /// <returns>Decrypted data of packet</returns>
        public static byte[] Decrypt(EncryptedPacket encryptedPacket)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="inputFile">File to read data from</param>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket EncryptFile(string inputFile, byte[] publicKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypt a file and store all data in another file
        /// </summary>
        /// <param name="inputFile">File to read data from</param>
        /// <param name="outputFile">Path to file to write packet to</param>
        public static void EncryptFile(string inputFile, string outputFile, byte[] publicKey)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt an encrypted packet of data to a file
        /// </summary>
        /// <param name="inputFile">File to read encrypted packet from</param>
        /// <returns>Decrypted data of packet</returns>
        public static byte[] DecryptFile(string inputFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt a file and write it to another file
        /// </summary>
        /// <param name="inputFile">File to read encrypted packet from</param>
        /// <param name="outputFile">File to write decrypted data to</param>
        public static void DecryptFile(string inputFile, string outputFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt an encrypted packet of data to a file
        /// </summary>
        /// <param name="encryptedPacket"></param>
        /// <param name="outputFile">File to write decrypted data to</param>
        public static void DecryptToFile(EncryptedPacket encryptedPacket, string outputFile)
        {
            throw new NotImplementedException();
        }
    }
}
