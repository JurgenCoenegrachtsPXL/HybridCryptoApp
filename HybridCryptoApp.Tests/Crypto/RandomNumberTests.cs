//using System;

using HybridCryptoApp.Crypto;
using NUnit.Framework;

namespace HybridCryptoApp.Tests.Crypto
{
    [TestFixture]
    public class RandomNumberTests
    {
        [Test]
        public void Random_Can_Generate_Array_Of_Bytes()
        {
            byte[] randomNumbers = Random.GetNumbers(32);

            Assert.NotNull(randomNumbers);
            CollectionAssert.IsNotEmpty(randomNumbers);
        }

        [Test]
        [TestCase(32)]
        [TestCase(128)]
        [TestCase(256)]
        public void Random_Can_Generate_Array_Of_Requested_Length(int length)
        {
            byte[] randomNumbers = Random.GetNumbers(length);

            Assert.That(randomNumbers.Length, Is.EqualTo(length));
        }

        [Test]
        public void Random_Generates_Different_Random_Numbers()
        {
            byte[] randomNumbers = Random.GetNumbers(32);
            byte[] randomNumbers2 = Random.GetNumbers(32);

            CollectionAssert.AreNotEqual(randomNumbers, randomNumbers2);
        }
    }
}
