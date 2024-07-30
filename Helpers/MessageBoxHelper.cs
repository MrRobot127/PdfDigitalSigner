using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PdfDigitalSigner.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowError(string message)
        {

            Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        public static void ShowWarning(string message)
        {
            Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void ShowInformation(string message)
        {
            Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowSuccess(string message)
        {
            Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage icon)
        {
            try
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    return MessageBox.Show(Application.Current.MainWindow, message, title, button, icon);
                });
            }
            catch
            {
                throw;
            }
        }
    }
}
