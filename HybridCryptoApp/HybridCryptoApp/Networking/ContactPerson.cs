using System.Collections.Generic;
using HybridCryptoApp.Networking.Models;
using HybridCryptoApp.Windows;

namespace HybridCryptoApp.Networking
{
    /// <summary>
    /// Contact/friend of the user
    /// </summary>
    public class ContactPerson
    {
        public string UserName => FirstName + " " + LastName;

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PublicKey { get; set; }

        public List<Message> Messages { get; set; } = new List<Message>();
        public string LastMessage { get; set; }
    }
}