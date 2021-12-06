using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for InstrumentBuilderControl.xaml
    /// </summary>
    public partial class InstrumentBuilderControl : UserControl
    {
        public InstrumentBuilderControl()
        {
            InitializeComponent();
            this.DataContext = IoC.Get<InstrumentBuilderViewModel>();
        }

        private void WaveView_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length < 1 || files[0].Length < 4 || files[0].Substring(files[0].Length - 3) != "wav")
                    return;

                IoC.Get<InstrumentBuilderViewModel>().LoadWave(files[0]);
            }
        }


        private void SingleMidiView_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length < 1 || files[0].Length < 4 || files[0].Substring(files[0].Length - 3) != "mid")
                    return;

                IoC.Get<InstrumentBuilderViewModel>().LoadMidi(files[0]);
            }
        }
    }
}
