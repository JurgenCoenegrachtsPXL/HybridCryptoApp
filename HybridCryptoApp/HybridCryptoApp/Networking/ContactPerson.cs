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

        protected bool Equals(ContactPerson other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ContactPerson) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id;
        }
    }
}