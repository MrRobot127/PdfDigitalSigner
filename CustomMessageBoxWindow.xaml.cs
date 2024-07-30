using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace PdfDigitalSigner
{
    public enum CustomMessageBoxResult
    {
        OK,
        Cancel,
        Closed,
        None
    }

    public partial class CustomMessageBoxWindow : Window
    {
        public CustomMessageBoxResult Result { get; private set; }

        public CustomMessageBoxWindow(string title, string message, bool showCancelButton, string okButtonText, string cancelButtonText)
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;

            TitleText.Text = title;
            SetIcon(title);
            MessageText.Text = message;

            OKButton.Content = okButtonText;
            OKButton.Visibility = Visibility.Visible;

            if (showCancelButton)
            {
                CancelButton.Content = cancelButtonText;
                CancelButton.Visibility = Visibility.Visible;
            }

            this.Loaded += CustomMessageBoxWindow_Loaded;

        }

        private void CustomMessageBoxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            var parentWindow = Application.Current.MainWindow;

            double parentLeft = parentWindow.Left;
            double parentTop = parentWindow.Top;
            double parentWidth = parentWindow.Width;
            double parentHeight = parentWindow.Height;

            double childWidth = this.ActualWidth;
            double childHeight = this.ActualHeight;


            double childLeft = parentLeft + (parentWidth - childWidth) / 2;
            double childTop = parentTop + (parentHeight - childHeight) / 2;

            this.Left = childLeft;
            this.Top = childTop;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CancelButton_Click(sender, e);
            }
        }


        public static CustomMessageBoxResult ManageMainWindow(string message)
        {
            var messageBox = new CustomMessageBoxWindow("Warning", message, true, "Yes", "No");
            messageBox.ShowDialog();
            return messageBox.Result;
        }

        public static void ShowError(string message, string iconName)
        {
            ShowMessage("Error", message, false, "OK", "");
        }

        private static void ShowMessage(string title, string message, bool showCancelButton, string okButtonText, string cancelButtonText)
        {
            var messageBox = new CustomMessageBoxWindow(title, message, showCancelButton, okButtonText, cancelButtonText);
            messageBox.ShowDialog();
        }

        private void SetIcon(string title)
        {
            string imagePath = $"Images/{title}.png";

            ErrorIcon.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SetResultAndClose(CustomMessageBoxResult.Closed);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            SetResultAndClose(CustomMessageBoxResult.OK);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetResultAndClose(CustomMessageBoxResult.Cancel);
        }

        private void SetResultAndClose(CustomMessageBoxResult result)
        {
            Result = result;
            this.Close();
        }
    }
}
