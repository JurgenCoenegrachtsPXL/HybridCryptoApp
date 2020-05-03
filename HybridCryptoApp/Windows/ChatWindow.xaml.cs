using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using HybridCryptoApp.Crypto;
using HybridCryptoApp.Networking;
using HybridCryptoApp.Networking.Models;
using Microsoft.Win32;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly ObservableCollection<ContactPerson> contactList = new ObservableCollection<ContactPerson>();
        public ContactPerson SelectedContact { get; set; }

        private List<StrippedDownEncryptedPacket> receivedPackets;
        private List<StrippedDownEncryptedPacket> sentPackets;
        private static readonly Regex IdRegex = new Regex(@"^\d+$");
        private DateTime lastUpdated = DateTime.Now;
        public string ErrorText { get; private set; } = "";
        private static readonly object lockObject = new object();

        public ChatWindow()
        {
            InitializeComponent();

            DataContext = this;
            ContactListListView.ItemsSource = contactList;

            ErrorTextBlock.DataContext = this;

            BindingOperations.EnableCollectionSynchronization(contactList, lockObject);

            // load all contacts and messages
            //new Action(async () => { await RetrieveAll(); })();
            /*
             Task.Run(async () =>
            {
                await RetrieveAll();
            }).Wait();
            */
            RetrieveAll().ConfigureAwait(false);

            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(3);
            dispatcherTimer.Tick += RefreshMessages;
            dispatcherTimer.Start();
        }

        /// <summary>
        /// Update contacts
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveContacts()
        {
            List<ContactPerson> currentContacts = contactList.ToList();
            List<ContactPerson> newContacts = await Client.GetAllContacts();

            // removed
            currentContacts.Except(newContacts)
                .ToList()
                .ForEach(c => contactList.Remove(c));

            // added
            List<Task> tasks = new List<Task>();

            newContacts.Except(currentContacts)
                .ToList()
                .ForEach(c =>
                {
                    contactList.Add(c);
                    tasks.Add(RetrieveMessagesOfContact(c.Id));
                });

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Retrieve all messages of a certain contact
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        private async Task<int> RetrieveMessagesOfContact(int contactId)
        {
            List<StrippedDownEncryptedPacket> packets = await Client.MessagesOfContact(contactId);

            return await AddReceivedMessages(packets);
        }

        /// <summary>
        /// Retrieve all messages from all contacts
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveAll()
        {
            lastUpdated = DateTime.Now;

            try
            {
                // get contacts
                (await Client.GetAllContacts()).ForEach(c => contactList.Add(c));

                // get all messages
                receivedPackets = (await Client.GetReceivedMessages()).ToList();
                sentPackets = (await Client.GetSentMessages()).ToList();
            }
            catch (ClientException exception)
            {
                ErrorText = exception.Message;
                return;
            }

            // try to link messages to contacts
            int failedDecryptionCount = await AddReceivedMessages(receivedPackets);

            failedDecryptionCount += await AddSentMessages(sentPackets);

            // sort all messages of all contacts
            foreach (ContactPerson contactPerson in contactList.AsEnumerable() ?? Enumerable.Empty<ContactPerson>())
            {
                CollectionViewSource.GetDefaultView(contactPerson.Messages).SortDescriptions.Add(new SortDescription(nameof(Message.SendTime), ListSortDirection.Ascending));
            }

            // alert user to possible errors
            if (failedDecryptionCount > 0)
            {
                ErrorText = $"Failed to decrypt {failedDecryptionCount} message(s).";
            }
        }

        /// <summary>
        /// Get newest messages
        /// </summary>
        /// <returns></returns>
        private async void RefreshMessages(object obj, EventArgs e)
        {
            // retrieve message from one second before last update
            DateTime startRequest = DateTime.Now;

            List<StrippedDownEncryptedPacket> received = await Client.GetReceivedMessagesAfter(lastUpdated.AddSeconds(-1));

            // update to the latest date to avoid requesting (many) duplicates if one packet was slowed down
            lastUpdated = (startRequest > lastUpdated) ? startRequest : lastUpdated;

            // try to link messages to contacts
            int failedCount = await AddReceivedMessages(received);
            
            // show user if any of these failed
            if (failedCount > 0)
            {
                ErrorText = $"Failed to decrypt {failedCount} message(s) on last refresh";
            }
        }

        /// <summary>
        /// Send a new message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ContactPerson contact = (ContactPerson)ContactListListView.SelectedItem;
            if (contact == null)
            {
                return;
            }

            EncryptedPacket packetForReceiver = null, packetForSender = null;
            string text = MessageTextBox.Text;
            await Task.Run(() =>
            {
                try
                {
                    packetForReceiver = HybridEncryption.Encrypt(DataType.Message, Encoding.UTF8.GetBytes(text), AsymmetricEncryption.PublicKeyFromXml(contact.PublicKey));
                    packetForSender = HybridEncryption.Encrypt(DataType.Message, Encoding.UTF8.GetBytes(text), AsymmetricEncryption.PublicKey);
                }
                catch (CryptoException exception)
                {
                    ErrorText = exception.Message;
                }
            });

            // try to send message and clear input
            if (packetForReceiver != null && packetForSender != null)
            {
                try
                {
                    // send to server
                    await Client.SendNewMessage(packetForReceiver, contact.Id, true);
                    await Client.SendNewMessage(packetForSender, contact.Id, false);

                    // add to chat
                    contact.Messages.Add(new Message()
                    {
                        SenderName = Client.UserName,
                        SendTime = DateTime.Now,
                        MessageFromSender = MessageTextBox.Text,
                        DataType = DataType.Message
                    });
                    MessageTextBox.Clear();
                }
                catch (ClientException exception)
                {
                    // show message to user
                    ErrorText = exception.Message;
                }
            }
        }

        /// <summary>
        /// Add a new contact to the list of contacts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddNewContactButton_Click(object sender, RoutedEventArgs e)
        {
            // get id or email from user
            PopupWindow popupWindow = new PopupWindow("User id or email");
            popupWindow.ShowDialog();
            string inputDataNewContactPerson = popupWindow.UserInputText;

            try
            {
                // find out if it's an id or an email
                if (IdRegex.IsMatch(inputDataNewContactPerson))
                {
                    // ID
                    await Client.AddContactById(int.Parse(inputDataNewContactPerson));
                }
                else
                {
                    // EMAIL
                    await Client.AddContactByEmail(inputDataNewContactPerson);
                }

                // update contacts
                await RetrieveContacts();
            }
            catch (ClientException exception)
            {
                // show to user
                ErrorText = exception.Message;
            }
        }

        /// <summary>
        /// Decrypt received file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send a file to currently selected user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendAttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            // get currently selected contact, do nothing if none is selected
            ContactPerson contact = (ContactPerson)ContactListListView.SelectedItem;
            if (contact == null)
            {
                return;
            }

            // ask user to select file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose a file to encrypt";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                FileStream fileStream;
                FileInfo fileInfo;

                try
                {
                    fileInfo = new FileInfo(openFileDialog.FileName);

                    if (fileInfo.Length > 10_000_000) // only upload files smaller than 10MB
                    {
                        ErrorText = "Due to server limitations, files bigger than 10MB (10 000 000 bytes) are not supported.";
                        return;
                    }

                    fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);

                    // encrypt file for receiver
                    EncryptedPacket packetForReceiver = null; //, packetForSender = null;
                    await Task.Run(() =>
                    {
                        try
                        {
                            packetForReceiver = HybridEncryption.EncryptFile(fileStream, AsymmetricEncryption.PublicKeyFromXml(contact.PublicKey));
                            //packetForReceiver = HybridEncryption.Encrypt(fileStream, AsymmetricEncryption.PublicKeyFromXml(contact.PublicKey));
                            //packetForSender = HybridEncryption.Encrypt(DataType.File, Encoding.UTF8.GetBytes(text), AsymmetricEncryption.PublicKey);
                        }
                        catch (CryptoException exception)
                        {
                            // show to user
                            ErrorText = exception.Message;
                        }
                    });

                    // send to receiver
                    await Client.SendNewMessage(packetForReceiver, contact.Id);
                }
                catch (IOException exception)
                {
                    // show to user
                    ErrorText = exception.Message;
                }
                catch (ClientException exception)
                {
                    // show to user
                    ErrorText = exception.Message;
                }
            }
        }

        /// <summary>
        /// Add received messages from list of packets
        /// </summary>
        /// <param name="received"></param>
        /// <returns>Amount of packages which failed to be decrypted</returns>
        private async Task<int> AddReceivedMessages(IList<StrippedDownEncryptedPacket> received)
        {
            // try to link messages to contacts
            BoxedInt boxedInt = new BoxedInt();
            List<Task> tasks = new List<Task>(received.Count);

            foreach (StrippedDownEncryptedPacket packet in received)
            {
                // find sender in contact list
                tasks.Add(Task.Run(() =>
                {
                    ContactPerson sender = contactList.FirstOrDefault(c => c.Id == packet.Sender.Id);
                    if (sender != null)
                    {
                        // ignore duplicates
                        if (sender.Messages.All(m => m.SendTime != packet.SendDateTime))
                        {
                            if (packet.DataType == DataType.Message)
                            {
                                try
                                {
                                    string message = Encoding.UTF8.GetString(HybridEncryption.Decrypt(packet.EncryptedPacket, AsymmetricEncryption.PublicKeyFromXml(sender.PublicKey)));

                                    // add new message to chat
                                    lock (sender.LockObject)
                                    {
                                        sender.Messages.Add(new Message()
                                        {
                                            SenderName = sender.UserName,
                                            SendTime = packet.SendDateTime,
                                            MessageFromSender = message,
                                            DataType = packet.DataType
                                        });
                                    }
                                }
                                catch (CryptoException)
                                {
                                    lock (boxedInt)
                                    {
                                        boxedInt.Integer++;
                                    }
                                }
                            }
                            else
                            {
                                lock (sender.LockObject)
                                {
                                    sender.Messages.Add(new Message()
                                    {
                                        SenderName = sender.UserName,
                                        SendTime = packet.SendDateTime,
                                        MessageFromSender = $"This is a {packet.DataType}",
                                        DataType = packet.DataType
                                    });
                                }
                            }
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            return boxedInt.Integer;
        }

        /// <summary>
        /// Add sent messages from list of packets
        /// </summary>
        /// <param name="sent"></param>
        /// <returns>Amount of packages which failed to be decrypted</returns>
        private async Task<int> AddSentMessages(IList<StrippedDownEncryptedPacket> sent)
        {
            // try to link messages to contacts
            BoxedInt boxedInt = new BoxedInt();
            List<Task> tasks = new List<Task>(sent.Count);

            foreach (StrippedDownEncryptedPacket packet in sentPackets)
            {
                // skip if current user is both the sender and receiver
                if (packet.Receiver.Id == packet.Sender.Id)
                {
                    continue;
                }

                tasks.Add(Task.Run(() =>
                {
                    // find receiver in contact list
                    ContactPerson receiver = contactList.FirstOrDefault(c => c.Id == packet.Receiver.Id);
                    if (receiver != null)
                    {
                        if (packet.DataType == DataType.Message)
                        {
                            try
                            {
                                string message = Encoding.UTF8.GetString(HybridEncryption.Decrypt(packet.EncryptedPacket, AsymmetricEncryption.PublicKey, true));

                                lock (receiver.LockObject)
                                {
                                    receiver.Messages.Add(new Message()
                                    {
                                        SenderName = Client.UserName,
                                        SendTime = packet.SendDateTime,
                                        MessageFromSender = message,
                                        DataType = packet.DataType
                                    });
                                }
                            }
                            catch (CryptoException)
                            {
                                lock (boxedInt)
                                {
                                    boxedInt.Integer++;
                                }
                            }
                        }
                        else
                        {
                            lock (receiver.LockObject)
                            {
                                receiver.Messages.Add(new Message()
                                {
                                    SenderName = receiver.UserName,
                                    SendTime = packet.SendDateTime,
                                    MessageFromSender = $"This is a {packet.DataType}",
                                    DataType = packet.DataType
                                });
                            }
                        }
                    }
                }));
            }

            // wait for all to finish
            await Task.WhenAll(tasks);

            return boxedInt.Integer;
        }

        /// <summary>
        /// Simply put an object around an integer
        /// </summary>
        private class BoxedInt
        {
            public int Integer { get; set; }
        }
    }
}
