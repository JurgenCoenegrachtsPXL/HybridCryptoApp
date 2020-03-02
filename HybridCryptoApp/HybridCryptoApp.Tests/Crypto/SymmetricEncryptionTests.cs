using HybridCryptoApp.Crypto;
using NUnit.Framework;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class SymmetricEncryptionTests
    {
        private byte[] key;
        private byte[] iv;

        [OneTimeSetUp]
        public void SetUp()
        {
            key = Random.GetNumbers(32);
            iv = Random.GetNumbers(32);
        }

        [Test]
        public void Can_Encrypt_Data()
        {
            byte[] rawData = Random.GetNumbers(256);

            byte[] encryptedData = SymmetricEncryption.Encrypt(rawData, key, iv);
            Assert.NotNull(encryptedData);
            CollectionAssert.IsNotEmpty(encryptedData);
        }

        [Test]
        public void Encrypted_Data_Differs_From_Raw_data()
        {
            byte[] rawData = Random.GetNumbers(256);

            byte[] encryptedData = SymmetricEncryption.Encrypt(rawData, key, iv);
            CollectionAssert.AreNotEqual(rawData, encryptedData);
        }

        [Test]
        public void Can_DecryptData()
        {
            byte[] rawData = Random.GetNumbers(256);

            byte[] decryptedData = SymmetricEncryption.Decrypt(rawData, key, iv);
            Assert.NotNull(decryptedData);
            CollectionAssert.IsNotEmpty(decryptedData);
        }

        [Test]
        public void Decrypted_Data_Differs_From_Raw_data()
        {
            byte[] rawData = Random.GetNumbers(256);

            byte[] decryptedData = SymmetricEncryption.Encrypt(rawData, key, iv);
            CollectionAssert.AreNotEqual(rawData, decryptedData);
        }

        [Test]
        public void Encrypted_Data_Can_Be_Decrypted()
        {
            byte[] rawData = Random.GetNumbers(256);

            byte[] encryptedData = SymmetricEncryption.Encrypt(rawData, key, iv);

            byte[] decryptedData = SymmetricEncryption.Decrypt(encryptedData, key, iv);

            CollectionAssert.AreEqual(rawData, decryptedData);
        }
    }
}
