using SpectralSynthesizer.Models;
using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for InstrumentBuilderDetailControl.xaml
    /// </summary>
    public partial class InstrumentBuilderDetailControl : UserControl
    {
        public InstrumentBuilderDetailControl()
        {
            InitializeComponent();
            this.DataContext = IoC.Get<InstrumentBuilderViewModel>();
            OnDataContextChanged();
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

        /// <summary>
        /// Called when the <see cref="UserControl.DataContext"/> property has changed.
        /// </summary>
        private void OnDataContextChanged()
        {
            var dc = DataContext as InstrumentBuilderViewModel;
            conversionAmplitudeSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            conversionLengthSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            conversionSilenceSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            sinusoidAmplitudeSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            sinusoidSleepingTimeSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            sinusoidMinimumLength.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            sinusoidContinuationRangeSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            transientStrengthSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            transientAdjacencyNumberSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            transientFlagRatioSlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
            noiseSamplingFrequencySlider.LabelledSliderDragDone += dc.SaveModelOnParametersChanged;
        }
    }
}
