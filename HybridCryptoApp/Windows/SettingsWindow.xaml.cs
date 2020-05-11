using System;
using System.Windows;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public static AesKeyLength AesKeyLength { get; private set; } = AesKeyLength.Normal;
        public static RsaKeyLength RsaKeyLength { get; private set; } = RsaKeyLength.Normal;
        public static bool UseDifferentRsaKeys { get; private set; } = false;

        public SettingsWindow()
        {
            InitializeComponent();
            AesComboBox.ItemsSource = Enum.GetValues(typeof(AesKeyLength));
            AesComboBox.SelectedItem = AesKeyLength;
            //AesComboBox.SelectedIndex = Enum.GetNames(typeof(AesKeyLength)).ToList().IndexOf(Enum.GetName(typeof(AesKeyLength), AesKeyLength));

            RsaComboBox.ItemsSource = Enum.GetValues(typeof(RsaKeyLength));
            RsaComboBox.SelectedItem = RsaKeyLength;
            //RsaComboBox.SelectedIndex = Enum.GetNames(typeof(RsaKeyLength)).ToList().IndexOf(Enum.GetName(typeof(RsaKeyLength), RsaKeyLength));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AesComboBox.SelectedIndex > -1)
            {
                AesKeyLength = (AesKeyLength)AesComboBox.SelectedItem;
            }

            if (RsaComboBox.SelectedIndex > -1)
            {
                RsaKeyLength = (RsaKeyLength)RsaComboBox.SelectedItem;
            }

            UseDifferentRsaKeys = UseDifferentRsaKeysCheckBox.IsChecked ?? false;

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    /// <summary>
    /// Length of AES key
    /// </summary>
    public enum AesKeyLength
    {
        VeryShort = 128,
        Short = 192,
        Normal = 256,
    }

    /// <summary>
    /// Length of RSA key
    /// </summary>
    public enum RsaKeyLength
    {
        VeryShort = 1024,
        Short = 2048,
        Normal = 4096,
        Long = 8192,
        VeryLong = 12_288,
        Extreme = 16_384
    }
}
