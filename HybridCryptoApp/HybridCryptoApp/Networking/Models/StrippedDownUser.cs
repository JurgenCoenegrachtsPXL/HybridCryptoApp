using System;
using HybridCryptoApp.Crypto;

namespace HybridCryptoApp.Networking.Models
{
    public class StrippedDownUser
    {
        /// <summary>
        /// Primary key in database of backend
        /// </summary>
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PublicKey { get; set; }
    }
}