using System;
using HybridCryptoApp.Crypto;
using NUnit.Framework;
using Random = HybridCryptoApp.Crypto.Random;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class HashingTests
    {
        private byte[] key;

        [OneTimeSetUp]
        public void Setup()
        {
            key = Random.GetNumbers(32);
        }

        [Test]
        public void Hashing_Can_Generate_Hash_Of_Message()
        {
            string message = "Hello World";
            byte[] hash = Hashing.HmacSha(message, key);

            Assert.NotNull(hash);
            CollectionAssert.IsNotEmpty(hash);
        }

        [Test]
        public void Hashing_Can_Generate_Same_Hash_For_Same_Message()
        {
            string message = "Hello World";
            byte[] hash1 = Hashing.HmacSha(message, key);
            byte[] hash2 = Hashing.HmacSha(message, key);

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Generates_Different_Hash_For_Different_Message()
        {
            string message1 = "Hello World";
            string message2 = "Goodbye World";

            byte[] hash1 = Hashing.HmacSha(message1, key);
            byte[] hash2 = Hashing.HmacSha(message2, key);

            CollectionAssert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Can_Generate_Hash_Of_Data()
        {
            byte[] data = Random.GetNumbers(256);
            byte[] hash = Hashing.HmacSha(data, key);

            Assert.NotNull(hash);
            CollectionAssert.IsNotEmpty(hash);
        }

        [Test]
        public void Hashing_Can_Generate_Same_Hash_For_Same_Data()
        {
            byte[] data = Random.GetNumbers(256);
            byte[] hash1 = Hashing.HmacSha(data, key);
            byte[] hash2 = Hashing.HmacSha(data, key);

            CollectionAssert.AreEqual(hash1, hash2);
        }

        [Test]
        public void Hashing_Generates_Different_Hash_For_Different_Data()
        {
            byte[] data1 = Random.GetNumbers(256);
            byte[] data2 = Random.GetNumbers(256);

            byte[] hash1 = Hashing.HmacSha(data1, key);
            byte[] hash2 = Hashing.HmacSha(data2, key);

            CollectionAssert.AreNotEqual(hash1, hash2);
        }

        [Test]
        public void Hash_Compare_Can_Compare_Hashes_Of_Equal_Length_Correctly()
        {
            int hashLength = 128;

            byte[] hash1 = Random.GetNumbers(hashLength);

            byte[] hash2 = new byte[hashLength];
            Buffer.BlockCopy(hash1, 0, hash2, 0, hash1.Length);

            Assert.True(Hashing.CompareHashes(hash1, hash2));
        }

        [Test]
        public void Hash_Compare_Can_Compare_Hashes_Of_Unequal_Length_Correctly()
        {
            int hashLength = 256;

            byte[] hash1 = Random.GetNumbers(hashLength);
            
            byte[] hash2 = new byte[hashLength - 1];
            Buffer.BlockCopy(hash1, 0, hash2, 0, hashLength - 1);

            Assert.False(Hashing.CompareHashes(hash1, hash2));
        }
    }
}
