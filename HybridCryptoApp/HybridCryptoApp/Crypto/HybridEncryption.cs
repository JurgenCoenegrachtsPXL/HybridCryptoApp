using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HybridCryptoApp.Crypto.Streamable;

namespace HybridCryptoApp.Crypto
{
    public class HybridEncryption
    {
        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="type">Type of data</param>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="publicKey">Public key of receiver</param>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket Encrypt(DataType type, byte[] data, RSAParameters publicKey)
        {
            // TODO: create EAS session key
            // TODO: create EAS IV
            // TODO: encrypt data with EAS
            // TODO: generate hash of encrypted data
            // TODO: encrypt EAS session key with RSA
            // TODO: put all info into new EncryptedPacket
            throw new NotImplementedException();
        }

        /// <summary>
        /// Decrypt an encrypted packet of data
        /// </summary>
        /// <param name="encryptedPacket"></param>
        /// <returns>Decrypted data of packet</returns>
        public static byte[] Decrypt(EncryptedPacket encryptedPacket)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="inputStream">File to read data from</param>
        /// <param name="publicKey"></param>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket EncryptFile(Stream inputStream, RSAParameters publicKey)
        {
            //FileStream inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
            MemoryStream outputStream = new MemoryStream();

            byte[] aesKey = Random.GetNumbers(32);
            byte[] iv = Random.GetNumbers(16);

            EncryptedPacket encryptedPacket = new EncryptedPacket
            {
                DataType = DataType.File, 
                EncryptedSessionKey = AsymmetricEncryption.Encrypt(aesKey, publicKey), 
                Iv = iv
            };

            // create streams
            using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            {
                using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
                {
                    var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                    var encryptedStream = symmetricStreamer.EncryptStream(hmacStream, CryptoStreamMode.Write);

                    // read all data
                    inputStream.CopyTo(encryptedStream);

                    // get hash
                    encryptedPacket.Hmac = hashStreamer.Hash;

                    // close file streams
                    inputStream.Close();
                }
            }

            // read encrypted data from memory stream
            encryptedPacket.EncryptedData = outputStream.ToArray();

            return encryptedPacket;
        }

        /// <summary>
        /// Encrypt a file and store all data in another file
        /// </summary>
        /// <param name="inputStream">Stream to read data from</param>
        /// <param name="outputStream">Stream to write encrypted packet to</param>
        /// <param name="publicKey"></param>
        public static void EncryptFile(Stream inputStream, Stream outputStream, RSAParameters publicKey)
        {
            byte[] aesKey = Random.GetNumbers(32);
            byte[] iv = Random.GetNumbers(16);

            // write header
            byte[] encryptedSessionKey = AsymmetricEncryption.Encrypt(aesKey, publicKey);

            List<byte> firstData = new List<byte>();
            firstData.Add((byte) DataType.File);
            firstData.AddRange(BitConverter.GetBytes((ushort) encryptedSessionKey.Length));
            firstData.AddRange(encryptedSessionKey);
            firstData.AddRange(iv);

            outputStream.Write(firstData.ToArray(), 0, firstData.Count); // write datatype, encrypted aes key and aes iv
            outputStream.Write(new byte[64], 0, 64); // reserve space for hmac

            // create streams
            using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            {
                using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
                {
                    var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                    var encryptedStream = symmetricStreamer.EncryptStream(hmacStream, CryptoStreamMode.Write);

                    // read all data
                    inputStream.CopyTo(encryptedStream);

                    // write hash in front of file
                    outputStream.Seek(firstData.Count, SeekOrigin.Begin);
                    outputStream.Write(hashStreamer.Hash, 0, hashStreamer.Hash.Length);
                    outputStream.Flush();

                    // close file streams
                    inputStream.Close();
                    outputStream.Close();
                }
            }
        }

        /// <summary>
        /// Decrypt a file and write it to another file
        /// </summary>
        /// <param name="inputStream">File to read encrypted packet from</param>
        /// <param name="outputStream">File to write decrypted data to</param>
        public static bool DecryptFile(Stream inputStream, Stream outputStream)
        {
            DataType dataType = (DataType)inputStream.ReadByte();
            
            // read aes key
            byte[] encryptedAesKeyLengthBuffer = new byte[2];
            inputStream.Read(encryptedAesKeyLengthBuffer, 0, 2);
            ushort encryptedAesKeyLength = BitConverter.ToUInt16(encryptedAesKeyLengthBuffer, 2);

            byte[] encryptedAesKey = new byte[encryptedAesKeyLength];
            inputStream.Read(encryptedAesKey, 0, encryptedAesKeyLength);
            byte[] aesKey = AsymmetricEncryption.Decrypt(encryptedAesKey);

            // read aes iv
            byte[] iv = new byte[16];
            inputStream.Read(iv, 0, 16);

            byte[] hmac = new byte[64];
            inputStream.Read(hmac, 0, 64);

            // create streams
            using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            {
                using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
                {
                    var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                    var decryptStream = symmetricStreamer.DecryptStream(hmacStream, CryptoStreamMode.Write);

                    // read all data
                    inputStream.CopyTo(decryptStream);
                    inputStream.Flush();

                    // close file streams
                    inputStream.Close();
                    outputStream.Close();

                    // check hash
                    return Hashing.CompareHashes(hashStreamer.Hash, hmac);
                }
            }
        }
    }
}
