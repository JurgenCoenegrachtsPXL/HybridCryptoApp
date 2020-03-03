using System;
using System.Collections.Generic;
using System.Linq;
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
        public static EncryptedPacket Encrypt(DataType type, byte[] data, byte[] publicKey)
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
    }
}
