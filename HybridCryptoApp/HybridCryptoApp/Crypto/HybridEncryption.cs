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

            // create streamers
            using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
            {
                // create streams
                var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                var encryptedStream = symmetricStreamer.EncryptStream(hmacStream, CryptoStreamMode.Write);

                // read all data
                inputStream.CopyTo(encryptedStream);

                // get hash
                encryptedPacket.Hmac = hashStreamer.Hash;

                // close file streams
                inputStream.Close();
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
        public static async Task<long> EncryptFile(Stream inputStream, Stream outputStream, RSAParameters publicKey)
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

            await outputStream.WriteAsync(firstData.ToArray(), 0, firstData.Count); // write datatype, encrypted aes key and aes iv
            await outputStream.FlushAsync();
            await outputStream.WriteAsync(new byte[64], 0, 64); // reserve space for hmac
            await outputStream.FlushAsync();
            // create streams
            //using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            {
                using (HashStreamer hashStreamer = new HashStreamer(aesKey))
                using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
                {
                    var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                    var encryptedStream = symmetricStreamer.EncryptStream(hmacStream, CryptoStreamMode.Write);
                    outputStream.SetLength(firstData.Count + 64);
                    outputStream.Position = firstData.Count + 64;

                    await inputStream.CopyToAsync(encryptedStream);

                    // write hash in front of file
                    outputStream.Position = 0;
                    await outputStream.WriteAsync(firstData.ToArray(), 0, firstData.Count);
                    await outputStream.WriteAsync(hashStreamer.Hash, 0, 64);
                    await outputStream.FlushAsync();
                    
                    return outputStream.Length;
                }
            }
        }

        /// <summary>
        /// Decrypt a file and write it to another file
        /// </summary>
        /// <param name="inputStream">File to read encrypted packet from</param>
        /// <param name="outputStream">File to write decrypted data to</param>
        public static async Task<bool> DecryptFile(Stream inputStream, Stream outputStream)
        {
            inputStream.Position = 0;
            DataType dataType = (DataType)inputStream.ReadByte();

            // read and decrypt aes key
            byte[] encryptedAesKeyLengthBuffer = new byte[2];
            await inputStream.ReadAsync(encryptedAesKeyLengthBuffer, 0, 2);
            await inputStream.FlushAsync();
            ushort encryptedAesKeyLength = BitConverter.ToUInt16(encryptedAesKeyLengthBuffer, 0);

            byte[] encryptedAesKey = new byte[encryptedAesKeyLength];
            await inputStream.ReadAsync(encryptedAesKey, 0, encryptedAesKeyLength);
            await inputStream.FlushAsync();
            byte[] aesKey = AsymmetricEncryption.Decrypt(encryptedAesKey);

            // read aes iv
            byte[] iv = new byte[16];
            await inputStream.ReadAsync(iv, 0, 16);
            await inputStream.FlushAsync();

            // read hash
            byte[] hmac = new byte[64];
            await inputStream.ReadAsync(hmac, 0, 64);
            await inputStream.FlushAsync();

            // create streamers
            using (HashStreamer hashStreamer = new HashStreamer(aesKey))
            using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
            {
                long currentPos = inputStream.Position;

                // create streams
                var hmacStream = hashStreamer.HmacShaStream(new MemoryStream(), aesKey, CryptoStreamMode.Write);
                var decryptStream = symmetricStreamer.DecryptStream(outputStream, CryptoStreamMode.Write);

                // create hash
                await inputStream.CopyToAsync(hmacStream);
                inputStream.Position = currentPos;
                await inputStream.FlushAsync();

                // skip decrypting if the hash isn't correct
                if (!Hashing.CompareHashes(hashStreamer.Hash, hmac))
                {
                    return false;
                }

                // decrypt the actual data
                await inputStream.CopyToAsync(decryptStream);
                await inputStream.FlushAsync();

                // Something something, absolute bullshit
                inputStream.Position = inputStream.Seek(-16, SeekOrigin.End);
                await inputStream.CopyToAsync(decryptStream);

                return true;
            }
        }
    }
}
