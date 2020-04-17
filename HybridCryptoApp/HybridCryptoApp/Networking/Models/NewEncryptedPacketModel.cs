using HybridCryptoApp.Crypto;

namespace HybridCryptoApp.Networking.Models
{
    public class NewEncryptedPacketModel : EncryptedPacket
    {
        public int ReceiverId { get; set; }

        public bool MeantForReceiver { get; set; } = true;

        /// <inheritdoc />
        public NewEncryptedPacketModel()
        {
        }

        /// <inheritdoc />
        public NewEncryptedPacketModel(EncryptedPacket encryptedPacket, int receiverId)
        {
            ReceiverId = receiverId;
            
            DataType = encryptedPacket.DataType;
            EncryptedData = encryptedPacket.EncryptedData;
            EncryptedSessionKey = encryptedPacket.EncryptedSessionKey;
            Hmac = encryptedPacket.Hmac;
            Iv = encryptedPacket.Iv;
            Signature = encryptedPacket.Signature;
        }
    }
}