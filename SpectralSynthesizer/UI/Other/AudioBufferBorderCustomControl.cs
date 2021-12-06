using SpectralSynthesizer.Models.DataModels.Enums;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SpectralSynthesizer"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SpectralSynthesizer;assembly=SpectralSynthesizer"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:AudioBufferBorderCustomControl/>
    ///
    /// </summary>
    public class AudioBufferBorderCustomControl : ContentControl
    {
        static AudioBufferBorderCustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AudioBufferBorderCustomControl), new FrameworkPropertyMetadata(typeof(AudioBufferBorderCustomControl)));
        }

        public ICommand LeftClickCommand
        {
            get { return (ICommand)GetValue(LeftClickCommandProperty); }
            set { SetValue(LeftClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftClickCommandProperty =
            DependencyProperty.Register("LeftClickCommand", typeof(ICommand), typeof(AudioBufferBorderCustomControl), new PropertyMetadata(null));

        public AudioBufferState State
        {
            get { return (AudioBufferState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(AudioBufferState), typeof(AudioBufferBorderCustomControl), new PropertyMetadata(AudioBufferState.Empty));


    }
}
