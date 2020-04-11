using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
                // ty to register
                await Client.Register(EmailTextBox.Text, PasswordTextBox.Text, FirstNameTextBox.Text, LastNameTextBox.Text);

                // TODO: show success message

                // wait 3 seconds
                await Task.Delay(3_000);

                // move to login window
                LoginWindow login = new LoginWindow(EmailTextBox.Text, PasswordTextBox.Text);
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
