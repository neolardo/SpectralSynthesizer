using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for AudioRendererControl.xaml
    /// </summary>
    public partial class AudioRendererControl : UserControl
    {
        public AudioRendererControl()
        {
            InitializeComponent();
            this.DataContext = IoC.Get<AudioRendererViewModel>();
        }

        private void MidiView_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length < 1 || files[0].Length < 4 || files[0].Substring(files[0].Length - 3) != "mid")
                    return;

                IoC.Get<AudioRendererViewModel>().LoadMidi(files[0]);
            }
        }
    }
}
