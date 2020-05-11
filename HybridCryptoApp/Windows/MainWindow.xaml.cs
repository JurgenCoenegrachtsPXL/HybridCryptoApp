using System.Windows;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.ShowDialog();
        }

        private void OfflineButton_Click(object sender, RoutedEventArgs e)
        {
            OfflineWindow offlineWindow = new OfflineWindow();
            offlineWindow.ShowDialog();
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();
            chatWindow.ShowDialog();
        }
    }
}
