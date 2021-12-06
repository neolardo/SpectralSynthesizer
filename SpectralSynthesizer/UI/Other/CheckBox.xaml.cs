using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for CheckBox.xaml
    /// </summary>
    public partial class CheckBox : UserControl
    {
        public CheckBox()
        {
            InitializeComponent();
            Text = "";
        }


        /// <summary>
        /// Fires off when the IsChecked porperty has changed
        /// </summary>
        /// <param name="newvalue"></param>
        public delegate void CheckChangedDelegate(bool newvalue);

        /// <summary>
        /// Fires off when the IsChecked property has changed
        /// </summary>
        public event CheckChangedDelegate CheckChanged;

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CheckBox), new PropertyMetadata(true));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CheckBox), new PropertyMetadata(""));


        public SolidColorBrush MainColor
        {
            get { return (SolidColorBrush)GetValue(MainColorProperty); }
            set { SetValue(MainColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register("MainColor", typeof(SolidColorBrush), typeof(CheckBox), new PropertyMetadata(null));

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set { SetValue(CheckedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Checked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(CheckBox), new PropertyMetadata(false));

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Enabled)
            {
                Checked ^= true;
                CheckChanged?.Invoke(Checked);
            }
        }
    }
}
