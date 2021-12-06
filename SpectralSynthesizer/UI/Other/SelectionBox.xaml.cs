using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for SelectionBox.xaml
    /// </summary>
    public partial class SelectionBox : UserControl
    {
        public SelectionBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Dependency Property to set the selectionbox height
        /// </summary>
        public double ScrollHeight
        {
            get { return (double)GetValue(ScrollHeightProperty); }
            set { SetValue(ScrollHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollHeightProperty =
            DependencyProperty.Register("ScrollHeight", typeof(double), typeof(SelectionBox), new PropertyMetadata(60.0));

        /// <summary>
        /// Dependency Property to set the selectionbox width
        /// </summary>
        public double ScrollWidth
        {
            get { return (double)GetValue(ScrollWidthProperty); }
            set { SetValue(ScrollWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScrollHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollWidthProperty =
            DependencyProperty.Register("ScrollWidth", typeof(double), typeof(SelectionBox), new PropertyMetadata(200.0));

    }
}
