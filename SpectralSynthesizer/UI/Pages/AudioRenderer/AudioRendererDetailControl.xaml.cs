using SpectralSynthesizer.Models;
using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for AudioRendererDetailControl.xaml
    /// </summary>
    public partial class AudioRendererDetailControl : UserControl
    {
        public AudioRendererDetailControl()
        {
            InitializeComponent();
            this.DataContext = IoC.Get<AudioRendererViewModel>();
        }

        /// <summary>
        /// Called when dropping an instrument file to the instruments <see cref="ComboBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="ComboBox"/>.</param>
        /// <param name="e">The drag event arguments.</param>
        private void InstrumentsCombo_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length < 1)
                    return;

                IoC.Get<ProjectModel>().Import(files[0]);
            }
        }
    }

}
