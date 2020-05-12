using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HybridCryptoApp.Crypto.Streamable;

namespace HybridCryptoApp.Crypto
{
    public static class HybridEncryption
    {
        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="type">Type of data</param>
        /// <param name="data">Data to be encrypted</param>
        /// <param name="publicKey">Public key of receiver</param>
        /// <exception cref="CryptoException"></exception>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket Encrypt(DataType type, byte[] data, RSAParameters publicKey)
        {
            // create AES session key
            byte[] sessionKey = Random.GetNumbers(32);

            // create AES IV
            byte[] iv = Random.GetNumbers(16);

            // encrypt AES session key with RSA
            byte[] encryptedSessionKey;
            try
            {
                encryptedSessionKey = AsymmetricEncryption.Encrypt(sessionKey, publicKey);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while encrypting AES session key", e);
            }

            // encrypt data with AES
            byte[] encryptedData;
            try
            {
                encryptedData = SymmetricEncryption.Encrypt(data, sessionKey, iv);
            }
            catch (NullReferenceException)
            {
                throw new CryptoException("Data was null");
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while encrypting data", e);
            }


            // generate hash of encrypted data with session key
            byte[] hash;
            try
            {
                hash = Hashing.HmacSha(encryptedData, sessionKey);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while hashing data", e);
            }


            // generate signature using hash
            byte[] signature;
            try
            {
                signature = AsymmetricEncryption.Sign(hash);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while creating signature", e);
            }

            // put all info into new EncryptedPacket
            var encryptedPacket = new EncryptedPacket
            {
                DataType = type,
                EncryptedSessionKey = encryptedSessionKey,
                Iv = iv,
                Hmac = hash,
                Signature = signature,
                EncryptedData = encryptedData
            };

            return encryptedPacket;
            
        }

        /// <summary>
        /// Decrypt an encrypted packet of data
        /// </summary>
        /// <param name="encryptedPacket">Packet containing data</param>
        /// <param name="publicKey">Public RSA key of sender</param>
        /// <param name="skipSignature"></param>
        /// <exception cref="CryptoException"></exception>
        /// <returns>Decrypted data of packet</returns>
        public static byte[] Decrypt(EncryptedPacket encryptedPacket, RSAParameters publicKey, bool skipSignature = false)
        {
			// decrypt AES session key with private RSA key
            byte[] sessionKey;
            try
            {
                sessionKey = AsymmetricEncryption.Decrypt(encryptedPacket.EncryptedSessionKey);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while decryption AES session key", e);
            }

            // rehash data with session key
            byte[] hashedData;
            try
            {
                hashedData = Hashing.HmacSha(encryptedPacket.EncryptedData, sessionKey);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while hashing data", e);
            }

            // check hash
            bool checkedHash = Hashing.CompareHashes(hashedData, encryptedPacket.Hmac);
            
            if (!checkedHash)
            {
                throw new CryptoException("Hash validation failed, data may have been modified!");
            }

            // check signature if required
            if (!skipSignature)
            {
                bool checkedSignature;
                try
                {
                    checkedSignature = AsymmetricEncryption.CheckSignature(encryptedPacket.Signature, publicKey, encryptedPacket.Hmac);
                }
                catch (CryptographicException e)
                {
                    throw new CryptoException("Error while checking signature", e);
                }

                if (!checkedSignature)
                {
                    throw new CryptoException("Signature check failed, packet may have come from a different sender.");
                }
            }
            
            // decrypt data with AES key and IV
            try
            {
                return SymmetricEncryption.Decrypt(encryptedPacket.EncryptedData, sessionKey, encryptedPacket.Iv);
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while decrypting data", e);
            }
        }

        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="inputStream">File to read data from</param>
        /// <param name="publicKey"></param>
        /// <returns>EncryptedPacket with all info for receiver to decrypt</returns>
        public static EncryptedPacket EncryptFile(Stream inputStream, RSAParameters publicKey)
        {
            MemoryStream outputStream = new MemoryStream();

            byte[] aesKey = Random.GetNumbers(32);
            byte[] iv = Random.GetNumbers(16);

            EncryptedPacket encryptedPacket = new EncryptedPacket
            {
                DataType = DataType.File, 
                EncryptedSessionKey = AsymmetricEncryption.Encrypt(aesKey, publicKey), 
                Iv = iv
            };

            try
            {
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

                    // create signature
                    encryptedPacket.Signature = AsymmetricEncryption.Sign(encryptedPacket.Hmac);

                    // close file streams
                    inputStream.Close();
                }
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while encrypting stream", e);
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
        /// <returns>Length of output stream</returns>
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

            int signatureLength = publicKey.Modulus.Length;
            await outputStream.WriteAsync(new byte[signatureLength], 0, signatureLength); // reserve space for signature
            await outputStream.FlushAsync();

            try
            {
                using (HashStreamer hashStreamer = new HashStreamer(aesKey))
                using (SymmetricStreamer symmetricStreamer = new SymmetricStreamer(aesKey, iv))
                {
                    var hmacStream = hashStreamer.HmacShaStream(outputStream, aesKey, CryptoStreamMode.Write);
                    var encryptedStream = symmetricStreamer.EncryptStream(hmacStream, CryptoStreamMode.Write);
                    outputStream.SetLength(firstData.Count + 64 + signatureLength);
                    outputStream.Position = firstData.Count + 64 + signatureLength;

                    await inputStream.CopyToAsync(encryptedStream);

                    // write hash in front of file
                    outputStream.Position = 0;
                    await outputStream.WriteAsync(firstData.ToArray(), 0, firstData.Count);
                    byte[] hash = hashStreamer.Hash;
                    await outputStream.WriteAsync(hash, 0, 64);

                    // write signature in front of file
                    await outputStream.WriteAsync(AsymmetricEncryption.Sign(hash), 0, signatureLength);

                    // flush it all the way
                    await outputStream.FlushAsync();

                    return outputStream.Length;
                }
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while encrypting file", e);
            }
        }

        /// <summary>
        /// Decrypt a file and write it to another file
        /// </summary>
        /// <param name="inputStream">File to read encrypted packet from</param>
        /// <param name="outputStream">File to write decrypted data to</param>
        /// <param name="publicKey">Public key of sender</param>
        public static async Task<bool> DecryptFile(Stream inputStream, Stream outputStream, RSAParameters publicKey)
        {
            // roll back stream to start
            inputStream.Position = 0;

            // read data type
            inputStream.ReadByte();

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

            // read signature
            int signatureLength = AsymmetricEncryption.PublicKey.Modulus.Length;
            byte[] signature = new byte[signatureLength];
            await inputStream.ReadAsync(signature, 0, signatureLength);
            await inputStream.FlushAsync();

            // check signature
            if (!AsymmetricEncryption.CheckSignature(signature, publicKey, hmac))
            {
                throw new CryptoException("Signature check failed, file could have been sent by somebody else!");
            }

            try
            {
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

                    // flush it all
                    await outputStream.FlushAsync();

                    return true;
                }
            }
            catch (CryptographicException e)
            {
                throw new CryptoException("Error while decrypting stream", e);
            }
        }
    }
}
