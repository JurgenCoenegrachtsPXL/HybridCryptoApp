using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HybridCryptoApp.Networking.Models;
using Newtonsoft.Json;
using HybridCryptoApp.Crypto;
using System.Collections.Generic;
using System;

namespace HybridCryptoApp.Networking
{
    public static class Client
    {
        // relative paths in api
        private const string NewMessagePath = "/api/Message/NewMessage";
        private const string AsReceiverPath = "/api/Message/AsReceiver";
        private const string AsSenderPath = "/api/Message/AsSender";
        private const string AddContactPath = "/api/UserContact/add";
        private const string RemoveContactPath = "/api/UserContact/remove";
        private const string RegistrationPath = "/api/Authentication/register";
        private const string LoginPath = "/api/Authentication/token";

        private static readonly HttpClient HttpClient = new HttpClient()
        {
            BaseAddress = new Uri(Properties.Settings.Default.BaseAddress)
        };

        /// <summary>
        /// Sets the base address for requests
        /// </summary>
        public static string BaseAddress
        {
            set
            {
                Uri newAddress = new Uri(value);

                if (newAddress.IsWellFormedOriginalString() && newAddress.IsAbsoluteUri)
                {
                    HttpClient.BaseAddress = newAddress;
                    Properties.Settings.Default.BaseAddress = value;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    throw new ClientException("Invalid URL");
                }
            }
        }

        /// <summary>
        /// Login user via their email password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public static async Task Login(string email, string password)
        {
            HttpContent messageContent = Stringify(new LoginModel()
            {
                Email = email,
                Password = password
            });

            var response = await HttpClient.PostAsync(LoginPath, messageContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string token = await response.Content.ReadAsStringAsync();
                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer ", token);
            }
            else
            {
                throw new ClientException($"Couldn't log in, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task Register(string email, string password, string firstName, string lastName)
        {
            HttpContent messageContent = Stringify(new RegistrationModel()
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName
            });

            var response = await HttpClient.PostAsync(RegistrationPath, messageContent);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't register, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Send a message to a specified receiver
        /// </summary>
        /// <param name="encryptedPacket"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        public static async Task SendNewMessage(EncryptedPacket encryptedPacket, int receiverId)
        {
            HttpContent messageContent = Stringify(new NewEncryptedPacketModel(encryptedPacket, receiverId));

            var response = await HttpClient.PostAsync(NewMessagePath, messageContent);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't send new message, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Get all messages which this user has received
        /// </summary>
        /// <returns>List of packets this user has received</returns>
        public static async Task<List<StrippedDownEncryptedPacket>> GetReceivedMessages()
        {
            var response = await HttpClient.GetAsync(AsReceiverPath);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't get received messages, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            try
            {
                string content = await response.Content.ReadAsStringAsync();
                List<StrippedDownEncryptedPacket> packets = JsonConvert.DeserializeObject<List<StrippedDownEncryptedPacket>>(content);
                return packets;
            }
            catch(JsonException e)
            {
                throw new ClientException("Failed to read list of messages", e);
            }
        }

        /// <summary>
        /// Get all messages which this user has sent
        /// </summary>
        /// <returns>List of all packets which this user has sent to other users</returns>
        public static async Task<List<StrippedDownEncryptedPacket>> GetSentMessages()
        {
            var response = await HttpClient.GetAsync(AsSenderPath);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't get sent messages, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            try
            {
                string content = await response.Content.ReadAsStringAsync();
                List<StrippedDownEncryptedPacket> packets = JsonConvert.DeserializeObject<List<StrippedDownEncryptedPacket>>(content);
                return packets;
            }
            catch (JsonException e)
            {
                throw new ClientException("Failed to read list of messages", e);
            }
        }

        /// <summary>
        /// Add a new contact to contact list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task AddContact(int id)
        {
            HttpContent content = Stringify(new UserContactModel()
            {
                ContactId = id
            });

            var response = await HttpClient.PostAsync(AddContactPath, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't add contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Remove an existing contact from contact list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task RemoveContact(int id)
        {
            HttpContent content = Stringify(new UserContactModel()
            {
                ContactId = id
            });

            var response = await HttpClient.PostAsync(RemoveContactPath, content);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't remove contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Get all contacts of user
        /// </summary>
        /// <returns>List of all contacts of this user</returns>
        public static async Task GetAllContacts()
        {
            throw new NotImplementedException("Backend doesn't support this yet");
        }

        /// <summary>
        /// Change public key of user
        /// </summary>
        /// <param name="publicKey">XML representation of public key</param>
        /// <returns></returns>
        public static async Task ChangePublicKey(string publicKey)
        {
            throw new NotImplementedException("Backend doesn't support this yet");
        }

        /// <summary>
        /// Attempt to convert object into a string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static HttpContent Stringify(object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}