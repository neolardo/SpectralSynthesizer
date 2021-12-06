using SpectralSynthesizer.UI;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for SpectogramView.xaml
    /// </summary>
    public partial class SpectogramView : ScrollableZoomableView
    {
        public SpectogramView()
        {
            InitializeComponent();
            scrollViewer.ScrollChanged += (a, b) => frequencyRulerScrollViewer.ScrollToVerticalOffset(b.VerticalOffset);
            ScrollViewer = scrollViewer;
        }

        /// <summary>
        /// Moves the base scrollbar on keyboard scroll.
        /// </summary>
        private void frequencyRulerScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) => OnPreviewMouseWheel(sender, e);

    }
}
