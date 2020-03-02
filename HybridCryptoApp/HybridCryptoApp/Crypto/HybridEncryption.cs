using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridCryptoApp.Crypto
{
    public class HybridEncryption
    {
        public static EncryptedPacket Encrypt(DataType type, byte[] data)
        {
            // TODO: create EAS session key
            // TODO: create EAS IV
            // TODO: encrypt data with EAS
            // TODO: generate hash of encrypted data
            // TODO: encrypt EAS session key with RSA
            // TODO: put all info into new packet
            throw new NotImplementedException();
        }

        public static byte[] Decrypt(EncryptedPacket encryptedPacket)
        {
            throw new NotImplementedException();
        }
    }
}
