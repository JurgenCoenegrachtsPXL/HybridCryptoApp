namespace HybridCryptoApp.Crypto
{
    public class EncryptedPacket
    {
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
    }

    public enum DataType : byte
    {
        Message,
        File,
        Steganography
    }
}
