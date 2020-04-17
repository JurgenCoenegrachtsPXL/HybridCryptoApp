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
        /// <summary>
        /// Is the user currently logged in
        /// </summary>
        public static bool IsLoggedIn { get; private set; } = false;

        /// <summary>
        /// Name of user who is currently logged in
        /// </summary>
        public static string UserName { get; set; } = "";

        // relative paths in api
        private const string NewMessagePath = "/api/Message/NewMessage";
        private const string AsReceiverPath = "/api/Message/AsReceiver";
        private const string AsSenderPath = "/api/Message/AsSender";
        private const string MessagesOfContactPath = "/api/Message/OfContact/";

        private const string AddContactByIdPath = "/api/UserContact/addById";
        private const string AddContactByEmailPath = "/api/UserContact/addByEmail";
        private const string RemoveContactPath = "/api/UserContact/remove";
        private const string AllContactsPath = "/api/UserContact/all";

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
                string content = await response.Content.ReadAsStringAsync();
                LoginResponseModel loginResponse = JsonConvert.DeserializeObject<LoginResponseModel>(content);

                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResponse.Token);
                UserName = loginResponse.Name;
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
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="publicKey">XML representation of public key</param>
        /// <returns></returns>
        public static async Task Register(string email, string password, string firstName, string lastName, string publicKey)
        {
            HttpContent messageContent = Stringify(new RegistrationModel()
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                PublicKey = publicKey
            });

            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync(RegistrationPath, messageContent);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            } 

            // check statuscode
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
        /// <param name="meantForReceiver"></param>
        /// <returns></returns>
        public static async Task SendNewMessage(EncryptedPacket encryptedPacket, int receiverId, bool meantForReceiver = true)
        {
            HttpContent messageContent = Stringify(new NewEncryptedPacketModel(encryptedPacket, receiverId){MeantForReceiver = meantForReceiver});

            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync(NewMessagePath, messageContent);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

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
            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(AsReceiverPath);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            // check status code
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't get received messages, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            // try to convert response to list of packages
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
            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(AsSenderPath);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            // check status code
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't get sent messages, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            // try to convert response to list of packages
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
        public static async Task AddContactById(int id)
        {
            HttpContent content = Stringify(new UserContactModel()
            {
                ContactId = id
            });

            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync(AddContactByIdPath, content);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't add contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        public static async Task AddContactByEmail(string email)
        {
            HttpContent content = Stringify(new UserContactModel()
            {
                ContactEmail = email
            });

            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync(AddContactByEmailPath, content);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

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

            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.PostAsync(RemoveContactPath, content);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't remove contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }
        }

        /// <summary>
        /// Get all contacts of user
        /// </summary>
        /// <returns>List of all contacts of this user</returns>
        public static async Task<List<ContactPerson>> GetAllContacts()
        {
            // try to send to server
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(AllContactsPath);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't remove contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<ContactPerson>>(content);
        }

        /// <summary>
        /// Get all messages of a contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public static async Task<List<StrippedDownEncryptedPacket>> MessagesOfContact(int contactId)
        {
            HttpResponseMessage response;
            try
            {
                response = await HttpClient.GetAsync(MessagesOfContactPath + contactId);
            }
            catch (HttpRequestException e)
            {
                throw new ClientException("Failed to contact server", e);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ClientException($"Couldn't get messages of contact, code: {response.StatusCode} reason: " + await response.Content.ReadAsStringAsync());
            }

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<StrippedDownEncryptedPacket>>(content);
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