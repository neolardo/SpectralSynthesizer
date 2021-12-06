using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for ComboBox.xaml
    /// </summary>
    public partial class ComboBox : UserControl
    {
        public ComboBox()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Dependency Property to indicate wheter this combobox is enaled or not
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(ComboBox), new PropertyMetadata(true));


        public bool IsScrollOnlyVisibleOnHover
        {
            get { return (bool)GetValue(IsScrollOnlyVisibleOnHoverProperty); }
            set { SetValue(IsScrollOnlyVisibleOnHoverProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsScrollOnlyVisibleOnHover.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsScrollOnlyVisibleOnHoverProperty =
            DependencyProperty.Register("IsScrollOnlyVisibleOnHover", typeof(bool), typeof(ComboBox), new PropertyMetadata(true));


        public double ComboWidth
        {
            get { return (double)GetValue(ComboWidthProperty); }
            set { SetValue(ComboWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComboWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComboWidthProperty =
            DependencyProperty.Register("ComboWidth", typeof(double), typeof(ComboBox), new PropertyMetadata(300.0));

        public double ComboMinimumWidth
        {
            get { return (double)GetValue(ComboMinimumWidthProperty); }
            set { SetValue(ComboMinimumWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComboMinimumWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComboMinimumWidthProperty =
            DependencyProperty.Register("ComboMinimumWidth", typeof(double), typeof(ComboBox), new PropertyMetadata(10.0));

        public double ComboHeight
        {
            get { return (double)GetValue(ComboHeightProperty); }
            set { SetValue(ComboHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComboHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComboHeightProperty =
            DependencyProperty.Register("ComboHeight", typeof(double), typeof(ComboBox), new PropertyMetadata(120.0));


    }
}
