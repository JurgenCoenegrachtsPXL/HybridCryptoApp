using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HybridCryptoApp.Crypto.Streamable;
using HybridCryptoApp.Crypto;
using NUnit.Framework;
using Random = HybridCryptoApp.Crypto.Random;

namespace HybridCryptoApp.Tests.Crypto.Streamable
{
    [TestFixture]
    public class SymmetricStreamerTests
    {
        private SymmetricStreamer streamer;
        private byte[] key;
        private byte[] iv;

        [SetUp]
        public void SetUp()
        {
            key = Random.GetNumbers(32);
            iv = Random.GetNumbers(16);
            streamer = new SymmetricStreamer(key, iv);
        }

        [TearDown]
        public void TearDown()
        {
            streamer?.Dispose();
        }

        [Test]
        public void Can_Encrypt_Stream()
        {
            // raw data
            byte[] rawData = Random.GetNumbers(4096);
            MemoryStream rawDataStream = new MemoryStream(rawData);

            // attempt to encrypt, put encrypted stream into a memory stream and then put that stream into an array
            CryptoStream encryptedStream = streamer.EncryptStream(rawDataStream, CryptoStreamMode.Read);
            MemoryStream encryptedMemoryStream = new MemoryStream();
            encryptedStream.CopyTo(encryptedMemoryStream);
            byte[] encryptedBytes = encryptedMemoryStream.ToArray();

            // decrypt with known good decryptor
            byte[] decryptedBytes = SymmetricEncryption.Decrypt(encryptedBytes, key, iv);

            CollectionAssert.AreEqual(rawData, decryptedBytes);
        }

        [Test]
        public void Can_Decrypt_Stream()
        {
            // raw data
            byte[] rawData = Random.GetNumbers(4096);
            
            // encrypt with known good encryptor
            byte[] encryptedBytes = SymmetricEncryption.Encrypt(rawData, key, iv);
            MemoryStream encryptedMemoryStream = new MemoryStream(encryptedBytes);
            
            // attempt to decrypt, then put decrypted stream into a memory stream and put that stream into an array
            CryptoStream decryptedStream = streamer.DecryptStream(encryptedMemoryStream, CryptoStreamMode.Read);
            MemoryStream decryptedMemoryStream = new MemoryStream();
            decryptedStream.CopyTo(decryptedMemoryStream);
            byte[] decryptedBytes = decryptedMemoryStream.ToArray();

            CollectionAssert.AreEqual(rawData, decryptedBytes);
        }
    }
}
