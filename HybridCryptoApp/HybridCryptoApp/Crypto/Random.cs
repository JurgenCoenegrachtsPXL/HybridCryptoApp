using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HybridCryptoApp.Crypto
{
    public class Random
    {
        public static byte[] GetNumbers(int length)
        {
            using (var random = new RNGCryptoServiceProvider())
            {
                byte[] array = new byte[length];

                random.GetBytes(array);

                return array;
            }
        }
    }
}
