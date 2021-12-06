using SpectralSynthesizer.UI;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for WaveView.xaml
    /// </summary>
    public partial class WaveView : ScrollableZoomableView
    {
        public WaveView()
        {
            InitializeComponent();
            ScrollViewer = scrollViewer;
        }
    }
}
