using SpectralSynthesizer.UI;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for InstrumentView.xaml
    /// </summary>
    public partial class InstrumentView : View
    {
        public InstrumentView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged()
        {
            base.OnDataContextChanged();
            var dc = DataContext as InstrumentViewViewModel;
            grid.SizeChanged += dc.OnContentSizeChanged;
            //trigger size changed to initiliaze the view parameters
            dc.OnContentSizeChanged(grid, null);
        }

    }
}
