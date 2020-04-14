using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using HybridCryptoApp.Crypto;
using HybridCryptoApp.Networking;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // make sure user can't press button multiple times
            RegisterButton.IsEnabled = false;

            try
            {
                // load given key
                AsymmetricEncryption.SelectKeyPair(RSAKeyTextBox.Text, 4096);

                List<Task> tasks = new List<Task>();

                // try to register
                tasks.Add(Client.Register(EmailTextBox.Text, PasswordTextBox.Password, FirstNameTextBox.Text, LastNameTextBox.Text, AsymmetricEncryption.PublicKeyAsXml()));

                // wait at least 3 seconds
                tasks.Add(Task.Delay(3_000));

                // wait for 3 second timer and registration to complete, whichever finishes last
                await Task.WhenAll(tasks);

                // move to login window
                LoginWindow login = new LoginWindow(EmailTextBox.Text, PasswordTextBox.Password);
                login.Show();
                this.Close();
            }
            catch (ClientException exception)
            {
                // show error message
                ErrorLabel.Visibility = Visibility.Visible;
                ErrorLabel.Content = exception.Message;
            }

            // re-enable user input
            RegisterButton.IsEnabled = true;
        }
    }
}
