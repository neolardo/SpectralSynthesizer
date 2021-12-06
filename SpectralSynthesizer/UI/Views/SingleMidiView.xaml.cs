using SpectralSynthesizer.UI;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for SingleMidiView.xaml
    /// </summary>
    public partial class SingleMidiView : ScrollableZoomableView
    {
        public SingleMidiView()
        {
            InitializeComponent();
            ScrollViewer = scrollViewer;
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

    }
}
