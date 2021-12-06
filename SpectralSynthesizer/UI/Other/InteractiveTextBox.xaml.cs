using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for InteractiveTextBox.xaml
    /// </summary>
    public partial class InteractiveTextBox : UserControl
    {
        public InteractiveTextBox()
        {
            InitializeComponent();
            RightClickCommand = new RelayCommand(() => IsReadOnly = !IsReadOnly);
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(InteractiveTextBox), new PropertyMetadata(true));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(InteractiveTextBox), new PropertyMetadata(string.Empty));



        public int MaximumTextLength
        {
            get { return (int)GetValue(MaximumTextLengthProperty); }
            set { SetValue(MaximumTextLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumTextLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumTextLengthProperty =
            DependencyProperty.Register(nameof(MaximumTextLength), typeof(int), typeof(InteractiveTextBox), new PropertyMetadata(100));



        public ICommand RightClickCommand
        {
            get { return (ICommand)GetValue(RightClickCommandProperty); }
            set { SetValue(RightClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightClickCommandProperty =
            DependencyProperty.Register(nameof(RightClickCommand), typeof(ICommand), typeof(InteractiveTextBox), new PropertyMetadata(null));

    }
}
