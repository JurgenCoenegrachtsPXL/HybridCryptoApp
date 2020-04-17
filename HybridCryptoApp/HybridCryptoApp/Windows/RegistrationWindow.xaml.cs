using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private static Regex emailRegex = new Regex(@"^.+@.+\..+$");
        private static Regex passwordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // make sure user can't press button multiple times
            RegisterButton.IsEnabled = false;

            // check email
            if (!emailRegex.IsMatch(EmailTextBox.Text.Trim()))
            {
                ErrorLabel.Content = "Invalid email.";
                RegisterButton.IsEnabled = true;
                return;
            }

            // check password
            if (!passwordRegex.IsMatch(PasswordTextBox.Password.Trim()))
            {
                ErrorLabel.Content = "Invalid password. Password should contain at least 1 upper case letter and 1 number";
                RegisterButton.IsEnabled = true;
                return;
            }

            try
            {
                // load given RSA container
                await Task.Run(() => { AsymmetricEncryption.SelectKeyPair(RSAKeyTextBox.Text, 4096); });
                
                List<Task> tasks = new List<Task>();

                // try to register
                tasks.Add(Client.Register(EmailTextBox.Text, PasswordTextBox.Password.Trim(), FirstNameTextBox.Text.Trim(), LastNameTextBox.Text.Trim(), AsymmetricEncryption.PublicKeyAsXml()));

                // wait at least 3 seconds
                tasks.Add(Task.Delay(3_000));

                // wait for 3 second timer and registration to complete, whichever finishes last
                await Task.WhenAll(tasks);

                // move to login window
                LoginWindow login = new LoginWindow(EmailTextBox.Text.Trim(), PasswordTextBox.Password, RSAKeyTextBox.Text);
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
