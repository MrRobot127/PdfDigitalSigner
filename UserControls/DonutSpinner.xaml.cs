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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PdfDigitalSigner.UserControls
{
    public partial class DonutSpinner : UserControl
    {
        private DispatcherTimer dotTimer;
        private string loadingText = "Loading";
        private int dotCount = 0;
        public DonutSpinner()
        {
            InitializeComponent();

            dotTimer = new DispatcherTimer();
            dotTimer.Interval = TimeSpan.FromSeconds(0.5);
            dotTimer.Tick += DotTimer_Tick;
            dotTimer.Start();
        }

        private void DotTimer_Tick(object sender, EventArgs e)
        {
            // Increment dot count and update loading text
            dotCount++;
            loadingText = "Loading" + new string('.', dotCount % 4);

            // Set the text
            RunningDots.Text = loadingText;
        }

        public Duration Duration
        {
            get { return (Duration)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(Duration), typeof(DonutSpinner), new PropertyMetadata(default(Duration)));

        public Brush SpinnerColor
        {
            get { return (Brush)GetValue(SpinnerColorProperty); }
            set { SetValue(SpinnerColorProperty, value); }
        }

        public static readonly DependencyProperty SpinnerColorProperty =
            DependencyProperty.Register("SpinnerColor", typeof(Brush), typeof(DonutSpinner), new PropertyMetadata(Brushes.DodgerBlue));
    }
}
