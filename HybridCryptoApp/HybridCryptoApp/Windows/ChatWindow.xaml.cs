using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HybridCryptoApp.Crypto;
using HybridCryptoApp.Networking;
using HybridCryptoApp.Networking.Models;

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


        public ChatWindow()
        {
            InitializeComponent();

            DataContext = this;
            ContactListListView.ItemsSource = contactList;

            // load all contacts and messages
            new Action(async () => { await RetrieveAll(); })();
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
        /// 
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        private async Task RetrieveMessagesOfContact(int contactId)
        {
            List<StrippedDownEncryptedPacket> packets = await Client.MessagesOfContact(contactId);

            ContactPerson contact = contactList.First(c => c.Id == contactId);
            packets.ForEach(e => contact.Messages.Add(new Message()
            {
                SenderName = e.Sender.FirstName + " " + e.Sender.LastName,
                SendTime = e.SendDateTime,
                MessageFromSender = Encoding.UTF8.GetString(HybridEncryption.Decrypt(e.EncryptedPacket, AsymmetricEncryption.PublicKeyFromXml(contact.PublicKey)))
            }));
        }

        /// <summary>
        /// Retrieve all messages from all contacts
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveAll()
        {
            try
            {
                // get contacts
                (await Client.GetAllContacts()).ForEach(c => contactList.Add(c));
                
                // get all messages
                receivedPackets = await Client.GetReceivedMessages();
                sentPackets = await Client.GetSentMessages();
            }
            catch (ClientException exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            // try to link messages to contacts
            foreach (StrippedDownEncryptedPacket packet in receivedPackets)
            {
                // find sender in contact list
                ContactPerson sender = contactList.FirstOrDefault(c => c.Id == packet.Sender.Id);
                if (sender != null)
                {
                    sender.Messages.Add(new Message()
                    {
                        SenderName = sender.UserName,
                        SendTime = packet.SendDateTime,
                        MessageFromSender = Encoding.UTF8.GetString(HybridEncryption.Decrypt(packet.EncryptedPacket, AsymmetricEncryption.PublicKeyFromXml(sender.PublicKey)))
                    });
                }
            }

            foreach (StrippedDownEncryptedPacket packet in sentPackets)
            {
                // skip if current user is both the sender and receiver
                if (packet.Receiver.Id == packet.Sender.Id)
                {
                    continue;
                }

                // find sender in contact list
                ContactPerson receiver = contactList.FirstOrDefault(c => c.Id == packet.Sender.Id);
                if (receiver != null)
                {
                    receiver.Messages.Add(new Message()
                    {
                        SenderName = receiver.UserName,
                        SendTime = packet.SendDateTime,
                        MessageFromSender = Encoding.UTF8.GetString(HybridEncryption.Decrypt(packet.EncryptedPacket, AsymmetricEncryption.PublicKey, true))
                    });
                }
            }

            // sort all messages of all contacts
            foreach (ContactPerson contactPerson in contactList.AsEnumerable() ?? Enumerable.Empty<ContactPerson>())
            {
                contactPerson.Messages.Sort();
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

            EncryptedPacket packet = null;
            string text = MessageTextBox.Text;
            await Task.Run(() =>
            {
                packet = HybridEncryption.Encrypt(DataType.Message, Encoding.UTF8.GetBytes(text), AsymmetricEncryption.PublicKeyFromXml(contact.PublicKey));
            });

            // try to send message and clear input
            try
            {
                await Client.SendNewMessage(packet, contact.Id);
                MessageTextBox.Clear();
            }
            catch (ClientException exception)
            {
                // TODO: show message to user
                MessageBox.Show(exception.Message);
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
                // TODO: show to user
                MessageBox.Show(exception.Message);
            }
        }
    }
}
