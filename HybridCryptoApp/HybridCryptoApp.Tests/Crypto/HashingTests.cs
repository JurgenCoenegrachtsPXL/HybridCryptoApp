using HybridCryptoApp.Crypto;
using NUnit.Framework;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class HashingTests
    {
        [Test]
        public void Hashing_Can_Generate_Hash_Of_Message()
        {
            string message = "Hello World";
            byte[] hash = Hashing.Hash(message);

            Assert.NotNull(hash);
            CollectionAssert.IsNotEmpty(hash);
        }

        [Test]
        public void Hashing_Can_Generate_Same_Hash_For_Same_Message()
        {
            string message = "Hello World";
            byte[] hash1 = Hashing.Hash(message);
            byte[] hash2 = Hashing.Hash(message);

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Generates_Different_Hash_For_Different_Message()
        {
            string message1 = "Hello World";
            string message2 = "Goodbye World";

            byte[] hash1 = Hashing.Hash(message1);
            byte[] hash2 = Hashing.Hash(message2);

            CollectionAssert.AreNotEqual(hash1, hash2);
        }
    }
}
