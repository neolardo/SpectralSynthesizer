using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for PlaceholderTextGrid.xaml
    /// </summary>
    public partial class PlaceholderTextGrid : UserControl
    {
        public PlaceholderTextGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The placeholder text.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PlaceholderTextGrid), new PropertyMetadata("PLACEHOLDER"));

    }
}
