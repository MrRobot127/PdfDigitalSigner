using Microsoft.Win32;
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

namespace PdfDigitalSigner
{
    public partial class CertificateDetailWindow : Window
    {
        public CertificateDetailWindow()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;

            this.Loaded += CertificateDetailWindow_Loaded;
        }

        private void CertificateDetailWindow_Loaded(object sender, RoutedEventArgs e)
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

        public string CertificatePassword { get; private set; }
        public string CertificateFilePath { get; private set; }

        public new double Width { get; private set; }
        public new double Height { get; private set; }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateFileNameTextBlock.Text == "No File Chosen")
            {
                AlertMessage.Text = "Please select Signing Certificate (.pfx).";
                return;
            }
            else
            {
                CertificateFilePath = CertificateFileNameTextBlock.Text;
            }

            if (CertificatePasswordBox.Password == "")
            {
                AlertMessage.Text = "Please Enter Certificate Password.";
                return;
            }
            else
            {
                CertificatePassword = CertificatePasswordBox.Password;
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CertificateUploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Certificate files (*.pfx)|*.pfx|All files (*.*)|*.*";
            openFileDialog.Title = "Select a certificate file";

            if (openFileDialog.ShowDialog().HasValue)
            {
                string selectedFileName = openFileDialog.FileName;
                string extension = System.IO.Path.GetExtension(selectedFileName);

                if (extension.ToLower() == ".pfx")
                {
                    CertificateFileNameTextBlock.Text = selectedFileName;
                    CertificateFilePath = selectedFileName;
                    AlertMessage.Text = "";
                }
                else
                {
                    CertificateFileNameTextBlock.Text = "No File Chosen";
                    AlertMessage.Text = "Please select Signing Certificate (.pfx).";
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //DragMove();
        }

        private void CertificatePasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CertificateFormSubmitButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Cancel_Click(sender, e);
            }
        }
    }
}
