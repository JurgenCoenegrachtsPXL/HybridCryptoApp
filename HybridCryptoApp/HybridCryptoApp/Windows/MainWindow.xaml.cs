using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HybridCryptoApp.Crypto;
using Microsoft.Win32;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string FileFormat = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private async void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Hidden;
            StatusImage.Visibility = Visibility.Hidden;

            if (UserInputRSADetails())
            {
                return;
            }

            // ask user for file to encrypt
            string originalFile = null;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose a file to encrypt";

            if (openFileDialog.ShowDialog() == true)
            {
                originalFile = openFileDialog.FileName;
            }

            // ask user for location to save encrypted file
            string encryptedFile = null;
            
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save as";
            saveFileDialog.Filter = "Encrypted file (*.crypto)|*.crypto"; // TODO: verzin betere file extensie
            
            if (saveFileDialog.ShowDialog() == true)
            {
                encryptedFile = saveFileDialog.FileName;
            }

            // only open filestreams if files were selected by user
            if (originalFile != null && encryptedFile != null)
            {
                // open streams
                FileStream inputStream = null;
                FileStream outputStream = null;

                try
                {
                    inputStream = File.OpenRead(originalFile);
                    outputStream = File.OpenWrite(encryptedFile);

                    await HybridEncryption.EncryptFile(inputStream, outputStream, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text));
                    MessageBox.Show("Done"); // TODO: tell user their encryption is done
                }
                catch (IOException exception)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    ErrorTextBlock.Text = exception.Message;
                }
                finally
                {
                    inputStream?.Close();
                    outputStream?.Close();
                }
            }
        }

        private async void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Hidden;
            StatusImage.Visibility = Visibility.Hidden;

            if (UserInputRSADetails())
            {
                return;
            }

            // ask user for file to decrypt
            string originalFile = null;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose a file to decrypt";
            openFileDialog.Filter = "Decrypted file (*.crypto)|*.crypto|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                originalFile = openFileDialog.FileName;
            }

            // ask user for location to save decrypted file
            string decryptedFile = null;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save as";

            if (saveFileDialog.ShowDialog() == true)
            {
                decryptedFile = saveFileDialog.FileName;
            }

            // only open filestreams if files were selected by user
            if (originalFile != null && decryptedFile != null)
            {
                // open streams
                FileStream inputStream = null;
                FileStream outputStream = null;

                try
                {
                    inputStream = File.OpenRead(originalFile);
                    outputStream = File.OpenWrite(decryptedFile);

                    bool succeeded = await HybridEncryption.DecryptFile(inputStream, outputStream);
                    if (succeeded)
                    {
                        StatusImage.Visibility = Visibility.Visible;
                        StatusImage.Source = new BitmapImage(new Uri(@"/Images/checkmark.png", UriKind.Relative));
                    }
                    else
                    {
                        StatusImage.Visibility = Visibility.Visible;
                        StatusImage.Source = new BitmapImage(new Uri(@"/Images/redx.png", UriKind.Relative)); ;
                    }
                }
                catch (IOException exception)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    ErrorTextBlock.Text = exception.Message;
                }
                finally
                {
                    inputStream?.Close();
                    outputStream?.Close();
                }
            }
        }

        /// <summary>
        /// Set clipboard to public RSA key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportPublicRSAKey_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AsymmetricEncryption.PublicKeyAsXml());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void LoadPrivateKey(object sender, RoutedEventArgs routedEventArgs)
        {
            //AsymmetricEncryption.CreateNewKeyPair(PrivateRSAContainerName.Name, 4096);
            AsymmetricEncryption.SelectKeyPair(PrivateRSAContainerName.Text.Trim(), 4096);
        }

        /// <summary>
        /// Create a new RSA key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateRSAKey_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PrivateRSAContainerName.Text))
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Provide a valid container name";
                return;
            }

            AsymmetricEncryption.CreateNewKeyPair(PrivateRSAContainerName.Text.Trim(), 4096);
        }

        private async void EncryptMessageButton_Click(object sender, RoutedEventArgs e)
        {
            StatusImage.Visibility = Visibility.Hidden;
            ErrorTextBlock.Visibility = Visibility.Hidden;

            if (UserInputRSADetails()) 
            {
                return;
            }

            // ask user for location to save encrypted message
            string encryptedFile = null;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save as";
            saveFileDialog.Filter = "Encrypted message (*.crypto)|*.crypto";

            if (saveFileDialog.ShowDialog() == true)
            {
                encryptedFile = saveFileDialog.FileName;
            }

            // only open filestreams if files were selected by user
            if (encryptedFile != null)
            {
                // open streams
                MemoryStream inputStream = new MemoryStream();
                byte[] messageBytes = Encoding.UTF8.GetBytes(InputMessageBox.Text);
                inputStream.Write(messageBytes, 0, messageBytes.Length);
                inputStream.Position = 0;
                FileStream outputStream = null;

                try
                {
                    outputStream = File.OpenWrite(encryptedFile);

                    await HybridEncryption.EncryptFile(inputStream, outputStream, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text));
                    MessageBox.Show("Done");
                }
                catch (IOException exception)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    ErrorTextBlock.Text = exception.Message;
                }
                finally
                {
                    inputStream?.Close();
                    outputStream?.Close();
                }
            }
        }

        private bool UserInputRSADetails()
        {
            // check if user has provided a private key container name
            if (string.IsNullOrWhiteSpace(PrivateRSAContainerName.Text))
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Name of RSA container hasn't been set";
                return true;
            }

            // check if user has provided an public key
            if (string.IsNullOrWhiteSpace(PublicRSAKeyReceiver.Text))
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Public RSA key of receiver hasn't been set";
                return true;
            }

            return false;
        }

        private async void DecryptMessageButton_Click(object sender, RoutedEventArgs e)
        {
            //bericht is leeg
            ErrorTextBlock.Visibility = Visibility.Hidden;
            StatusImage.Visibility = Visibility.Hidden;

            if (UserInputRSADetails())
            {
                return;
            }

            // ask user for file to decrypt
            string originalFile = null;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Choose a file to put decrypted message";
            openFileDialog.Filter = "Decrypted message (*.crypto)|*.crypto|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                originalFile = openFileDialog.FileName;
            }

            // only open filestreams if files were selected by user
            FileStream inputStream = null;
            MemoryStream outputStream = null;
            if (originalFile != null)
            {
                try
                {
                    inputStream = File.OpenRead(originalFile);
                    outputStream = new MemoryStream();

                    bool succeeded = await HybridEncryption.DecryptFile(inputStream, outputStream);
                    if (succeeded)
                    {
                        StatusImage.Visibility = Visibility.Visible;
                        StatusImage.Source = new BitmapImage(new Uri(@"/Images/checkmark.png", UriKind.Relative));

                        outputStream.Position = 0;
                        byte[] messageBuffer = new byte[outputStream.Length];
                        outputStream.Read(messageBuffer, 0, messageBuffer.Length);
                        string message = Encoding.UTF8.GetString(messageBuffer);
                        MessageBox.Show(message);
                    }
                    else
                    {
                        StatusImage.Visibility = Visibility.Visible;
                        StatusImage.Source = new BitmapImage(new Uri(@"/Images/redx.png", UriKind.Relative));
                    }
                }
                catch (IOException exception)
                {
                    ErrorTextBlock.Visibility = Visibility.Visible;
                    ErrorTextBlock.Text = exception.Message;
                }
                finally
                {
                    inputStream?.Close();
                    outputStream?.Close();
                }
            }
        }
    }
}
