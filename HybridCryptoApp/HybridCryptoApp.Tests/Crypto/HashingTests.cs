using HybridCryptoApp.Crypto;
using NUnit.Framework;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class HashingTests
    {
        private byte[] salt;
        private byte[] key;

        [OneTimeSetUp]
        public void Setup()
        {
            salt = Random.GetNumbers(32);
            key = Random.GetNumbers(16);
        }

        [Test]
        public void Hashing_Can_Generate_Hash_Of_Message()
        {
            string message = "Hello World";
            byte[] hash = Hashing.Argon2(message, salt, key);

            Assert.NotNull(hash);
            CollectionAssert.IsNotEmpty(hash);
        }

        [Test]
        public void Hashing_Can_Generate_Same_Hash_For_Same_Message()
        {
            string message = "Hello World";
            byte[] hash1 = Hashing.Argon2(message, salt, key);
            byte[] hash2 = Hashing.Argon2(message, salt, key);

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Generates_Different_Hash_For_Different_Message()
        {
            string message1 = "Hello World";
            string message2 = "Goodbye World";

            byte[] hash1 = Hashing.Argon2(message1, salt, key);
            byte[] hash2 = Hashing.Argon2(message2, salt, key);

            CollectionAssert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Can_Generate_Hash_Of_Data()
        {
            byte[] data = Random.GetNumbers(256);
            byte[] hash = Hashing.Argon2(data, salt, key);

            Assert.NotNull(hash);
            CollectionAssert.IsNotEmpty(hash);
        }

        [Test]
        public void Hashing_Can_Generate_Same_Hash_For_Same_Data()
        {
            byte[] data = Random.GetNumbers(256);
            byte[] hash1 = Hashing.Argon2(data, salt, key);
            byte[] hash2 = Hashing.Argon2(data, salt, key);

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Generates_Different_Hash_For_Different_Data()
        {
            byte[] data1 = Random.GetNumbers(256);
            byte[] data2 = Random.GetNumbers(256);

            byte[] hash1 = Hashing.Argon2(data1, salt, key);
            byte[] hash2 = Hashing.Argon2(data2, salt, key);

            CollectionAssert.AreNotEqual(hash1, hash2);
        }
    }
}
