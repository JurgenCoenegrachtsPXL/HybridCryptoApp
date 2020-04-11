namespace HybridCryptoApp.Networking.Models
{
    public class StrippedDownEncryptedPacket
    {
        /// <summary>
        /// Primary key of user
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Last name of user
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of user
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// User's public key in XML form
        /// </summary>
        public string PublicKey { get; set; }
    }
}