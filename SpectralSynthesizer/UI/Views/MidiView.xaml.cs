using SpectralSynthesizer.UI;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for MidiView.xaml.
    /// </summary>
    public partial class MidiView : ScrollableZoomableView
    {
        public MidiView()
        {
            InitializeComponent();
            scrollViewer.ScrollChanged += (a, b) => keyboardscrollviewer.ScrollToVerticalOffset(b.VerticalOffset);
            ScrollViewer = scrollViewer;
        }

        /// <summary>
        /// Moves the base scrollbar on keyboard scroll.
        /// </summary>
        private void keyboardscrollviewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) => OnPreviewMouseWheel(sender, e);

    }
}
