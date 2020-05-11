using System;
using System.Windows;
using System.Windows.Input;

namespace HybridCryptoApp.Windows
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public string UserInputText { get; set; } = "";

        public PopupWindow(string titleText)
        {
            InitializeComponent();

            TitleTextBlock.Content = titleText;

            Closed += PopupWindow_Closed;
        }

        private void PopupWindow_Closed(object sender, EventArgs e)
        {
            UserInputText = UserInput.Text;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Close();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
