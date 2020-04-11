using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public List<ContactPerson> ContactList { get; set; }
        public ContactPerson SelectedContact { get; set; }

        private List<StrippedDownEncryptedPacket> receivedPackets;
        private List<StrippedDownEncryptedPacket> sentPackets;


        public ChatWindow()
        {
            InitializeComponent();

            // load all contacts and messages
            Task.Run(async () => { await RetrieveMessages(); });
        }

        /// <summary>
        /// Retrieve all messages from all contacts
        /// </summary>
        /// <returns></returns>
        private async Task RetrieveMessages()
        {
            try
            {
                // get contacts
                ContactList = await Client.GetAllContacts();

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
                ContactPerson sender = ContactList.FirstOrDefault(c => c.Id == packet.Sender.Id);
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
                // find sender in contact list
                ContactPerson receiver = ContactList.FirstOrDefault(c => c.Id == packet.Sender.Id);
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

            // sort all messages
            ContactList.ForEach(c => c.Messages.Sort());
        }
    }
}
