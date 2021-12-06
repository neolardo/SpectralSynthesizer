using SpectralSynthesizer.UI;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for Ruler.xaml
    /// </summary>
    public partial class RulerView : ScrollableZoomableView
    {
        public RulerView()
        {
            InitializeComponent();
            ScrollViewer = scrollViewer;
        }
    }
}
