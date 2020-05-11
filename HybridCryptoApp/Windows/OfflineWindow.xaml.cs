using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using HybridCryptoApp.Crypto;
using Microsoft.Win32;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OfflineWindow : Window
    {
        public OfflineWindow()
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

                    bool succeeded = await HybridEncryption.DecryptFile(inputStream, outputStream, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text));
                    if (succeeded)
                    {
                        StatusImage.Visibility = Visibility.Visible;
                        StatusImage.Source = new BitmapImage(new Uri(@"/Images/checkmark.png", UriKind.Relative));
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
        /// Load private key from container
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        private void LoadPrivateKey(object sender, RoutedEventArgs routedEventArgs)
        {
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

                // message requires a minimum length, pad it with 32 0x00 bytes
                if (inputStream.Length < 32)
                {
                    inputStream.Write(new byte[32], 0, 32);
                }

                inputStream.Position = inputStream.Seek(0, SeekOrigin.Begin);
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

                    bool succeeded = await HybridEncryption.DecryptFile(inputStream, outputStream, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text));
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

        private async void EncryptFileStenographyButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Hidden;

            FileStream baseFile = null;
            FileStream saveFile = null;
            MemoryStream messageData = null;
            MemoryStream encryptedData = null;

            try
            {
                // put original message into a memory stream
                byte[] messageBytes = Encoding.UTF8.GetBytes(InputMessageBox.Text);
                messageData = new MemoryStream();
                messageData.Write(messageBytes, 0, messageBytes.Length);
                if (messageData.Length < 32) // make sure it's longer than the minimum length
                {
                    messageData.Write(new byte[32], 0, 32);
                }
                messageData.Position = 0;

                // select file to use as base
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Choose a file to put encrypted message";

                if (openFileDialog.ShowDialog() == true)
                {
                    baseFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                } 
                else
                {
                    return;
                }

                // select file to save
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save as";
                saveFileDialog.DefaultExt = openFileDialog.SafeFileName.Split('.').Last();
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == true)
                {
                    saveFile = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write);
                } 
                else
                {
                    return;
                }

                // encrypt message into memory stream
                encryptedData = new MemoryStream();
                await HybridEncryption.EncryptFile(messageData, encryptedData, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text)).ConfigureAwait(false);
                encryptedData.Position = 0;

                // write length to output
                byte[] baseFileBuffer = new byte[8];

                byte[] length = BitConverter.GetBytes(encryptedData.Length);
                for (int i = 0; i < 8; i++)
                {
                    await baseFile.ReadAsync(baseFileBuffer, 0, 8); // read 8 bytes of input
                    HideInBytes(baseFileBuffer, length[i]);
                    await saveFile.WriteAsync(baseFileBuffer, 0, 8); // write 8 bytes to output    
                }
                
                // hide message in output
                byte[] encryptedDataBuffer = new byte[1];
                for (int i = 0; i < encryptedData.Length; i++)
                {
                    await encryptedData.ReadAsync(encryptedDataBuffer, 0, 1);
                    await baseFile.ReadAsync(baseFileBuffer, 0, 8); // read 8 bytes of input
                    HideInBytes(baseFileBuffer, encryptedDataBuffer[0]);
                    await saveFile.WriteAsync(baseFileBuffer, 0, 8); // write 8 bytes to output
                }

                // write rest of file to output
                byte[] fileBuffer = new byte[1024];
                while (await baseFile.ReadAsync(fileBuffer, 0, 1024) > 0)
                {
                    await saveFile.WriteAsync(fileBuffer, 0, 1024);
                }

                await saveFile.FlushAsync().ConfigureAwait(false);
            }
            catch (IOException exception)
            {
                ErrorTextBlock.Text = exception.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            catch (CryptoException exception)
            {
                ErrorTextBlock.Text = exception.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                baseFile?.Close();
                saveFile?.Close();
                messageData?.Close();
                encryptedData?.Close();
            }
        }

        private async void DecryptFileStenographyButton_Click(object sender, RoutedEventArgs e)
        {
            FileStream stegFile = null;
            MemoryStream encryptedData = null;
            MemoryStream messageData = null;

            try
            {
                // open file
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Choose a file to put decrypted message";

                if (openFileDialog.ShowDialog() == true)
                {
                    stegFile = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                } 
                else
                {
                    return;
                }

                // get length
                byte[] stegBuffer = new byte[64];
                stegFile.Read(stegBuffer, 0, 64);

                byte[] lengthBuffer = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    lengthBuffer[i] = GlueByteTogether(stegBuffer.Skip(i * 8).Take(8).ToArray());
                }

                long length = BitConverter.ToInt64(lengthBuffer, 0);
                encryptedData = new MemoryStream();

                // read rest of bytes
                long current = 0;
                stegBuffer = new byte[8];
                while (current < length)
                {
                    current++;
                    
                    // read from input
                    await stegFile.ReadAsync(stegBuffer, 0, 8);

                    // convert to single byte
                    byte gluedByte = GlueByteTogether(stegBuffer);

                    // write byte to memorystream
                    await encryptedData.WriteAsync(new byte[] {gluedByte}, 0, 1);
                }

                // decrypt data
                messageData = new MemoryStream();
                await HybridEncryption.DecryptFile(encryptedData, messageData, AsymmetricEncryption.PublicKeyFromXml(PublicRSAKeyReceiver.Text));

                // show message to user
                string message = Encoding.UTF8.GetString(messageData.ToArray());
                MessageBox.Show(message);
            }
            catch (IOException exception)
            {
                ErrorTextBlock.Text = exception.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            catch (CryptoException exception)
            {
                ErrorTextBlock.Text = exception.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
            finally
            {
                stegFile?.Close();
                encryptedData?.Close();
                messageData?.Close();
            }
        }

        public static byte GlueByteTogether(byte[] input)
        {
            byte output = 0;

            output |= (byte) ((byte)(input[0] & 0b0000_0001) << 7);
            output |= (byte) ((byte)(input[1] & 0b0000_0001) << 6);
            output |= (byte) ((byte)(input[2] & 0b0000_0001) << 5);
            output |= (byte) ((byte)(input[3] & 0b0000_0001) << 4);
            output |= (byte) ((byte)(input[4] & 0b0000_0001) << 3);
            output |= (byte) ((byte)(input[5] & 0b0000_0001) << 2);
            output |= (byte) ((byte)(input[6] & 0b0000_0001) << 1);
            output |= (byte) ((byte)(input[7] & 0b0000_0001) << 0);

            return output;
        }

        public static void HideInBytes(byte[] buffer, byte byteToHide)
        {
            // clear last bit
            for (int i = 0; i < 8; i++)
            {
                buffer[i] &= 0b1111_1110;
            }

            buffer[0] |= (byte)((byte)(byteToHide & 0b1000_0000) >> 7);
            buffer[1] |= (byte)((byte)(byteToHide & 0b0100_0000) >> 6);
            buffer[2] |= (byte)((byte)(byteToHide & 0b0010_0000) >> 5);
            buffer[3] |= (byte)((byte)(byteToHide & 0b0001_0000) >> 4);
            buffer[4] |= (byte)((byte)(byteToHide & 0b0000_1000) >> 3);
            buffer[5] |= (byte)((byte)(byteToHide & 0b0000_0100) >> 2);
            buffer[6] |= (byte)((byte)(byteToHide & 0b0000_0010) >> 1);
            buffer[7] |= (byte)((byte)(byteToHide & 0b0000_0001) >> 0);
        }
    }
}
