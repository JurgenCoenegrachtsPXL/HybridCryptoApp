using System;
using HybridCryptoApp.Crypto;

namespace HybridCryptoApp.Networking.Models
{
    public class StrippedDownEncryptedPacket
    {
        /// <summary>
        /// Primary key of user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Sender of the packet
        /// </summary>
        public StrippedDownUser Sender { get; set; }

        /// <summary>
        /// Receiver of the packet
        /// </summary>
        public StrippedDownUser Receiver { get; set; }

        /// <summary>
        /// When packet was sent
        /// </summary>
        public DateTime SendDateTime { get; set; }

        /// <summary>
        /// Type of data in packet
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// EAS session key, encrypted with RSA
        /// </summary>
        public byte[] EncryptedSessionKey { get; set; }

        /// <summary>
        /// Initialization Vector for AES (adds entropy to process)
        /// </summary>
        public byte[] Iv { get; set; }

        /// <summary>
        /// Hash to verify sender and data integrity
        /// </summary>
        public byte[] Hmac { get; set; }

        /// <summary>
        /// Signature of sender
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Actual data encrypted with EAS
        /// </summary>
        public byte[] EncryptedData { get; set; }

        /// <summary>
        /// Convert StrippedDownEncryptedPacket into an EncryptedPacket
        /// </summary>
        public EncryptedPacket EncryptedPacket => new EncryptedPacket()
        {
            DataType = DataType,
            EncryptedData = EncryptedData,
            EncryptedSessionKey = EncryptedSessionKey,
            Hmac = Hmac,
            Iv = Iv,
            Signature = Signature,
        };
    }
}