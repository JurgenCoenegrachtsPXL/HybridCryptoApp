using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using HybridCryptoApp.Crypto;
using HybridCryptoApp.Networking;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(string email, string password, string rsaContainerName)
        {
            InitializeComponent();

            EmailTextBox.Text = email;
            PasswordPasswordBox.Password = password;
            RSAKeyTextBox.Text = rsaContainerName;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // make sure user can't press button multiple times
            LoginButton.IsEnabled = false;

            try
            {
                // load RSA key
                string containerName = RSAKeyTextBox.Text;
                await Task.Run(() => { AsymmetricEncryption.SelectKeyPair(containerName, 4096); });
                
                // log in and wait for at least 1.5s, whichever finishes last
                List<Task> tasks = new List<Task>();

                tasks.Add(Client.Login(EmailTextBox.Text, PasswordPasswordBox.Password));

                tasks.Add(Task.Delay(1_500));

                await Task.WhenAll(tasks);

                //move to chat window
                ChatWindow chat = new ChatWindow();
                chat.Show();
                this.Close();
            }
            catch (ClientException exception)
            {
                // show error message
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = exception.Message;
            }

            // re-enable user input
            LoginButton.IsEnabled = true;
        }
    }
}
