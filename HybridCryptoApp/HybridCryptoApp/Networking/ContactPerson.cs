namespace HybridCryptoApp.Networking
{
    /// <summary>
    /// Contact/friend of the user
    /// </summary>
    public class ContactPerson
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PublicKey { get; set; }
    }
}