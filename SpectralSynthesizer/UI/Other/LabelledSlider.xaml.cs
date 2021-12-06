using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for LabelledSlider.xaml
    /// </summary>
    public partial class LabelledSlider : UserControl
    {
        public LabelledSlider()
        {
            InitializeComponent();
        }

        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing events on drag done
        /// </summary>
        public delegate void LabelledSliderDragDoneDelegate();

        /// <summary>
        /// Fires off when this slider has dragged
        /// </summary>
        public event LabelledSliderDragDoneDelegate LabelledSliderDragDone;

        #endregion

        #region Dependency Properties


        /// <summary>
        /// The minimum possible slider value
        /// </summary>
        public double MinimumValue
        {
            get { return (double)GetValue(MinimumValueProperty); }
            set { SetValue(MinimumValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinimumValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.Register("MinimumValue", typeof(double), typeof(LabelledSlider), new PropertyMetadata(0.0));

        /// <summary>
        /// The maximum possible slider value
        /// </summary>
        public double MaximumValue
        {
            get { return (double)GetValue(MaximumValueProperty); }
            set { SetValue(MaximumValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.Register("MaximumValue", typeof(double), typeof(LabelledSlider), new PropertyMetadata(0.0));

        /// <summary>
        /// The current slider value
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(LabelledSlider), new PropertyMetadata(0.0));

        /// <summary>
        /// The visible text value of the slider
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LabelledSlider), new PropertyMetadata(""));

        /// <summary>
        /// The label of this slider
        /// </summary>
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(LabelledSlider), new PropertyMetadata(""));

        /// <summary>
        /// Indicates wheter this control is interactable or not
        /// </summary>
        public bool IsInteractable
        {
            get { return (bool)GetValue(IsInteractableProperty); }
            set { SetValue(IsInteractableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInteractable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInteractableProperty =
            DependencyProperty.Register("IsInteractable", typeof(bool), typeof(LabelledSlider), new PropertyMetadata(true));

        #endregion

        #region Methods

        /// <summary>
        /// Called on slider mouse leave to fire the <see cref="LabelledSliderDragDone"/> event
        /// </summary>
        private void slider_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
                LabelledSliderDragDone?.Invoke();
        }

        /// <summary>
        /// Called on slider mouse up to fire the <see cref="LabelledSliderDragDone"/> event
        /// </summary>
        private void slider_MouseUp(object sender, MouseEventArgs e)
        {
            LabelledSliderDragDone?.Invoke();
        }

        #endregion

    }
}
