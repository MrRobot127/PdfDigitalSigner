using PdfDigitalSigner.UserControls;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace PdfDigitalSigner
{
    public partial class MainWindow : Window
    {
        private double _mainWindowInitialMaxHeight;

        public MainWindow()
        {
            InitializeComponent();
            SizeChanged += MainWindow_SizeChanged;
            _mainWindowInitialMaxHeight = this.MinHeight;

            // Attach the Closing event handler
            Closing += MainWindow_Closing;

        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ExcelDataUploadAndPreview.ProgressBar.Visibility == Visibility.Visible)
            {
                if (CustomMessageBoxWindow.ManageMainWindow("An operation is still in progress. Do you want to abort the current operation?") == CustomMessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                if (CustomMessageBoxWindow.ManageMainWindow("Are you sure you want to close the application?") == CustomMessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ExcelDataUploadAndPreview.FindName("EmployeeDataScrollViewer") is ScrollViewer scrollViewer)
            {
                double heightDifference = e.NewSize.Height - _mainWindowInitialMaxHeight;
                scrollViewer.MaxHeight += heightDifference;
                _mainWindowInitialMaxHeight = e.NewSize.Height;
            }
        }
    }
}
