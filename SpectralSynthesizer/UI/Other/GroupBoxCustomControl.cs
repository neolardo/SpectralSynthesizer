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
    ///     <MyNamespace:GroupBoxContentControl/>
    ///
    /// </summary>
    public class GroupBoxCustomControl : ContentControl
    {
        static GroupBoxCustomControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupBoxCustomControl), new FrameworkPropertyMetadata(typeof(GroupBoxCustomControl)));
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(GroupBoxCustomControl), new PropertyMetadata("Groupbox sample title"));


        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(GroupBoxCustomControl), new PropertyMetadata(false));




        public bool IsInteractable
        {
            get { return (bool)GetValue(IsInteractableProperty); }
            set { SetValue(IsInteractableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInteractable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInteractableProperty =
            DependencyProperty.Register("IsInteractable", typeof(bool), typeof(GroupBoxCustomControl), new PropertyMetadata(true));



        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            set { SetValue(MarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Margin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.Register("Margin", typeof(Thickness), typeof(GroupBoxCustomControl), new PropertyMetadata(new Thickness()));




        public ICommand TitleLeftClickedCommand
        {
            get { return (ICommand)GetValue(TitleLeftClickedCommandProperty); }
            set { SetValue(TitleLeftClickedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TitleLeftClickedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleLeftClickedCommandProperty =
            DependencyProperty.Register("TitleLeftClickedCommand", typeof(ICommand), typeof(GroupBoxCustomControl), new PropertyMetadata(null));



        private void OnTitleLeftClick()
        {
            if (IsInteractable)
            {
                IsOpen = !IsOpen;
            }
        }
        public GroupBoxCustomControl()
        {
            TitleLeftClickedCommand = new RelayCommand(() => OnTitleLeftClick());
        }
    }
}
