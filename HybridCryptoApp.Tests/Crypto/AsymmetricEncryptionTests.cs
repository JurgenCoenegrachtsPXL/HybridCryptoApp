﻿using System.Security.Cryptography;
using HybridCryptoApp.Crypto;
using NUnit.Framework;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class AsymmetricEncryptionTests
    {
        private static string TestContainerName => "newTestContainer";
        private RSAParameters publicKey;

        [SetUp]
        public void SetUp()
        {
            publicKey = AsymmetricEncryption.CreateNewKeyPair(TestContainerName, 4096);
        }

        [Test]
        public void GenerateKeyPAir_Returns_Public_Key()
        {
            RSAParameters rsaParameters = AsymmetricEncryption.CreateNewKeyPair(TestContainerName, 4096);
            
            Assert.NotNull(rsaParameters);
        }

        [Test]
        public void GenerateKeyPAir_Returns_Valid_Public_Key()
        {
            RSAParameters rsaParameters = AsymmetricEncryption.CreateNewKeyPair(TestContainerName, 4096);

            // public exponent
            Assert.NotNull(rsaParameters.Exponent);
            CollectionAssert.IsNotEmpty(rsaParameters.Exponent);

            // modulus
            Assert.NotNull(rsaParameters.Modulus);
            CollectionAssert.IsNotEmpty(rsaParameters.Modulus);

            // private exponent
            Assert.IsNull(rsaParameters.D);
        }

        [Test]
        public void Can_Give_Public_Key_As_XML_String()
        {
            string xml = AsymmetricEncryption.PublicKeyAsXml();

            Assert.NotNull(xml);
            Assert.That(xml, Is.Not.Empty);
        }

        [Test]
        public void Can_Convert_XML_String_To_Public_Key()
        {
            string xml = AsymmetricEncryption.PublicKeyAsXml();

            RSAParameters rsaParameters = AsymmetricEncryption.PublicKeyFromXml(xml);

            Assert.NotNull(rsaParameters);
            CollectionAssert.AreEqual(publicKey.Exponent, rsaParameters.Exponent);
            CollectionAssert.AreEqual(publicKey.Modulus, rsaParameters.Modulus);
        }

        [Test]
        public void Can_Encrypt_Data()
        {
            byte[] rawData = Random.GetNumbers(128);
            
            byte[] encryptedData = AsymmetricEncryption.Encrypt(rawData, publicKey);

            Assert.NotNull(encryptedData);
            Assert.AreNotEqual(rawData, encryptedData);
        }

        [Test]
        public void Can_Decrypt_Encrypted_Data()
        {
            byte[] rawData = Random.GetNumbers(128);

            byte[] encryptedData = AsymmetricEncryption.Encrypt(rawData, publicKey);
            byte[] decryptedData = AsymmetricEncryption.Decrypt(encryptedData);

            Assert.NotNull(decryptedData);
            Assert.AreEqual(rawData, decryptedData);
        }

        [Test]
        public void SelectKeyPair_Should_Change_Public_Key()
        {
            RSAParameters beforeParams = AsymmetricEncryption.PublicKey;

            AsymmetricEncryption.SelectKeyPair("otherPair", 512);

            RSAParameters afterParams = AsymmetricEncryption.PublicKey;
            Assert.That(beforeParams, Is.Not.EqualTo(afterParams));
        }
    }
}
