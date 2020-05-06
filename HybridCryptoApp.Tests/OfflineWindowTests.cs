using HybridCryptoApp.Windows;
using NUnit.Framework;
using System.Linq;

namespace HybridCryptoApp.Tests
{
    [TestFixture]
    public class OfflineWindowTests
    {
        [Test]
        public void GlueByteTogether_Should_Return_4()
        {
            byte[] input = new byte[] { 0, 0, 0, 0, 0, 1, 0, 0 };

            byte output = OfflineWindow.GlueByteTogether(input);

            Assert.That(output, Is.EqualTo(4));
        }

        [Test]
        public void GlueByteTogether_Should_Return_4_When_Bits_Are_Inside_Other_Data()
        {
            byte[] input = new byte[] { 10, 20, 30, 40, 50, 61, 70, 80 };

            byte output = OfflineWindow.GlueByteTogether(input);

            Assert.That(output, Is.EqualTo(4));
        }

        [Test]
        public void HideInBytes_Should_Change_Bytes()
        {
            byte[] input = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            byte[] copyOfOriginal = input.ToArray();

            OfflineWindow.HideInBytes(input, 57);

            CollectionAssert.AreNotEqual(copyOfOriginal, input);
        }

        [Test]
        public void GlueByteTogether_Should_Read_Hidden_Input()
        {
            byte[] input = new byte[] { 10, 20, 30, 40, 50, 61, 70, 80 };
            byte number = 46;

            OfflineWindow.HideInBytes(input, number);
            byte foundNumber = OfflineWindow.GlueByteTogether(input);

            Assert.That(number, Is.EqualTo(foundNumber));
        }

    }
}