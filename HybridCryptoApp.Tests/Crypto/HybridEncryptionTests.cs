﻿using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HybridCryptoApp.Crypto;
using NUnit.Framework;
using Random = HybridCryptoApp.Crypto.Random;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class HybridEncryptionTests
    {
        private const string AsymmetricContainerName = "testContainerName";

        private RSAParameters asymmetricPublicKey;

        private EncryptedPacket ValidMessagePacket
        {
            get
            {
                string inputString = "This is another test string";
                byte[] messageBytes = Encoding.UTF8.GetBytes(inputString);
                return HybridEncryption.Encrypt(DataType.Message, messageBytes, asymmetricPublicKey);
            }
        }

        [SetUp]
        public void SetUp()
        {
            asymmetricPublicKey = AsymmetricEncryption.CreateNewKeyPair(AsymmetricContainerName, 4096);
        }

        [TearDown]
        public void TearDown()
        {
            AsymmetricEncryption.DeleteKey(AsymmetricContainerName);
        }

        [Test]
        public void Can_Encrypt_Message()
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes("This is a test string");

            EncryptedPacket encryptedPacket = null;
            Assert.DoesNotThrow(() =>
            {
                encryptedPacket = HybridEncryption.Encrypt(DataType.Message, messageBytes, asymmetricPublicKey);
            });

            Assert.NotNull(encryptedPacket);
            Assert.That(encryptedPacket.DataType, Is.EqualTo(DataType.Message), "EncryptedPacket DataType property not set correctly");
        }

        [Test]
        public void Can_Encrypt_Small_File()
        {
            byte[] fileBytes = Random.GetNumbers(256);

            EncryptedPacket encryptedPacket = null;
            Assert.DoesNotThrow(() =>
            {
                encryptedPacket = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            });

            Assert.NotNull(encryptedPacket);
            Assert.That(encryptedPacket.DataType, Is.EqualTo(DataType.File), "EncryptedPacket DataType property not set correctly");
        }

        [Test]
        public void Can_Encrypt_Big_File()
        {
            byte[] fileBytes = Random.GetNumbers(5_889_614);

            EncryptedPacket encryptedPacket = null;
            Assert.DoesNotThrow(() =>
            {
                encryptedPacket = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            });

            Assert.NotNull(encryptedPacket);
            Assert.That(encryptedPacket.DataType, Is.EqualTo(DataType.File), "EncryptedPacket DataType property not set correctly");
        }

        [Test]
        public void Can_Encrypt_Steganography_File()
        {
            byte[] fileBytes = Random.GetNumbers(2_726_403);

            EncryptedPacket encryptedPacket = null;
            Assert.DoesNotThrow(() =>
            {
                encryptedPacket = HybridEncryption.Encrypt(DataType.Steganography, fileBytes, asymmetricPublicKey);
            });

            Assert.NotNull(encryptedPacket);
            Assert.That(encryptedPacket.DataType, Is.EqualTo(DataType.Steganography), "EncryptedPacket DataType property not set correctly");
        }

        [Test]
        public void Encryption_Populates_All_Packet_Fields()
        {
            byte[] fileBytes = Random.GetNumbers(2048); 
            EncryptedPacket encryptedPacket = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);

            Assert.NotNull(encryptedPacket.EncryptedSessionKey, "AES key not set");
            CollectionAssert.IsNotEmpty(encryptedPacket.EncryptedSessionKey, "AES key not set");

            Assert.NotNull(encryptedPacket.Iv, "AES IV not set");
            CollectionAssert.IsNotEmpty(encryptedPacket.Iv, "AES IV not set");

            Assert.NotNull(encryptedPacket.Hmac, "HMAC not set");
            CollectionAssert.IsNotEmpty(encryptedPacket.Hmac, "HMAC not set");

            Assert.NotNull(encryptedPacket.EncryptedData, "Encrypted data not set");
            CollectionAssert.IsNotEmpty(encryptedPacket.EncryptedData, "Encrypted data not set");
        }

        [Test]
        public void Can_Recover_Encrypted_Message()
        {
            string inputString = "This is another test string";
            byte[] messageBytes = Encoding.UTF8.GetBytes(inputString);
            EncryptedPacket encryptedPacket = HybridEncryption.Encrypt(DataType.Message, messageBytes, asymmetricPublicKey);

            byte[] decryptedBytes = HybridEncryption.Decrypt(encryptedPacket, asymmetricPublicKey);
            string decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);

            Assert.AreEqual(inputString, decryptedMessage);
        }

        [Test]
        public void Can_Recover_Encrypted_File()
        {
            byte[] fileBytes = Random.GetNumbers(2048);
            EncryptedPacket encryptedPacket = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            
            byte[] decryptedBytes = HybridEncryption.Decrypt(encryptedPacket, asymmetricPublicKey);

            CollectionAssert.AreEqual(fileBytes, decryptedBytes);
        }

        [Test]
        public void Encryption_Generates_New_Iv_Each_Time()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            EncryptedPacket encryptedPacket1 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            EncryptedPacket encryptedPacket2 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);

            CollectionAssert.AreNotEqual(encryptedPacket1.Iv, encryptedPacket2.Iv);
        }

        [Test]
        public void Encryption_Generates_New_Aes_Key_Each_Time()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            EncryptedPacket encryptedPacket1 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            EncryptedPacket encryptedPacket2 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);

            CollectionAssert.AreNotEqual(encryptedPacket1.EncryptedSessionKey, encryptedPacket2.EncryptedSessionKey);
        }

        [Test]
        public void Encryption_Generates_Different_Encrypted_Data_Each_Time()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            EncryptedPacket encryptedPacket1 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            EncryptedPacket encryptedPacket2 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);

            CollectionAssert.AreNotEqual(encryptedPacket1.EncryptedData, encryptedPacket2.EncryptedData);
        }

        [Test]
        public void Encryption_Generates_Different_Hmac_Each_Time()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            EncryptedPacket encryptedPacket1 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            EncryptedPacket encryptedPacket2 = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);

            CollectionAssert.AreNotEqual(encryptedPacket1.Hmac, encryptedPacket2.Hmac);
        }

        [Test]
        public void Decryption_Throws_CryptoException_When_Hmac_Differs()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            EncryptedPacket encryptedPacket = HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey);
            encryptedPacket.Hmac[25] = (byte) ((encryptedPacket.Hmac[25] + 1) % 255);

            Assert.Throws(typeof(CryptoException), () =>
            {
                HybridEncryption.Decrypt(encryptedPacket, asymmetricPublicKey);
            });
        }

        [Test]
        public void Can_Encrypt_Stream_Of_Data()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            MemoryStream inputStream = new MemoryStream(fileBytes);

            EncryptedPacket encryptedPacket = HybridEncryption.EncryptFile(inputStream, asymmetricPublicKey);

            Assert.NotNull(encryptedPacket);
            CollectionAssert.AreNotEqual(fileBytes, encryptedPacket.EncryptedData);
        }

        [Test]
        public async Task Can_Decrypt_Stream_Of_Data()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            MemoryStream rawStream = new MemoryStream(fileBytes);

            MemoryStream encryptedBytes = new MemoryStream();
            await HybridEncryption.EncryptFile(rawStream, encryptedBytes, asymmetricPublicKey);

            encryptedBytes.Position = 0;

            MemoryStream decryptedStream = new MemoryStream();
            bool hashOk = await HybridEncryption.DecryptFile(encryptedBytes, decryptedStream, asymmetricPublicKey);

            decryptedStream.Position = 0;
            byte[] decryptedBytes = decryptedStream.ToArray();

            Assert.True(hashOk);
            CollectionAssert.IsNotEmpty(decryptedBytes);
            CollectionAssert.AreEqual(fileBytes, decryptedBytes);
        }

        [Test, Explicit]
        public async Task Can_Decrypt_Large_Stream_Of_Data()
        {
            byte[] fileBytes = Random.GetNumbers(128_000_000);

            MemoryStream rawStream = new MemoryStream(fileBytes);

            MemoryStream encryptedBytes = new MemoryStream();
            await HybridEncryption.EncryptFile(rawStream, encryptedBytes, asymmetricPublicKey);

            encryptedBytes.Position = 0;

            MemoryStream decryptedStream = new MemoryStream();
            bool hashOk = await HybridEncryption.DecryptFile(encryptedBytes, decryptedStream, asymmetricPublicKey);

            decryptedStream.Position = 0;
            byte[] decryptedBytes = decryptedStream.ToArray();

            Assert.True(hashOk);
            CollectionAssert.IsNotEmpty(decryptedBytes);
            CollectionAssert.AreEqual(fileBytes, decryptedBytes);
        }

        [Test]
        public void Encrypt_Should_Throw_Crypto_Exception_If_RSA_Public_Key_Is_Invalid()
        {
            byte[] fileBytes = Random.GetNumbers(2048);

            asymmetricPublicKey.Modulus = new byte[1];

            Assert.That(() => { HybridEncryption.Encrypt(DataType.File, fileBytes, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Encrypt_Should_Throw_Crypto_Exception_If_Data_Is_Null()
        {
            Assert.That(() => { HybridEncryption.Encrypt(DataType.File, null, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_RSA_Public_Key_Is_Invalid()
        {
            asymmetricPublicKey.Modulus = new byte[1];

            Assert.That(() => { HybridEncryption.Decrypt(ValidMessagePacket, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_EncryptedAesKey_Was_Changed()
        {
            EncryptedPacket packet = ValidMessagePacket;
            packet.EncryptedSessionKey = new byte[32];

            Assert.That(() => { HybridEncryption.Decrypt(packet, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_IV_Was_Changed()
        {
            EncryptedPacket packet = ValidMessagePacket;
            packet.Iv = new byte[32];

            Assert.That(() => { HybridEncryption.Decrypt(packet, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_Hash_Was_Changed()
        {
            EncryptedPacket packet = ValidMessagePacket;
            packet.Hmac = new byte[64];

            Assert.That(() => { HybridEncryption.Decrypt(packet, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }


        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_Signature_Was_Changed()
        {
            EncryptedPacket packet = ValidMessagePacket;
            packet.Hmac = new byte[128];

            Assert.That(() => { HybridEncryption.Decrypt(packet, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }

        [Test]
        public void Decrypt_Should_Throw_Crypto_Exception_If_Encrypted_Data_Was_Changed()
        {
            EncryptedPacket packet = ValidMessagePacket;
            packet.EncryptedData = new byte[256];

            Assert.That(() => { HybridEncryption.Decrypt(packet, asymmetricPublicKey); }, Throws.InstanceOf(typeof(CryptoException)));
        }
    }
}
