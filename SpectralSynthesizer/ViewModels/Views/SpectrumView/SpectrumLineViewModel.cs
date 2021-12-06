using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of a <see cref="Models.Audio.Data.Spectrum"/>'s line. 
    /// </summary>
    public class SpectrumLineViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The width of this line in pixels.
        /// </summary>
        public double Width { get; set; }

        private double _height = SpectrumViewViewModel.LineMinimumHeight;

        /// <summary>
        /// The height of this line in pixels.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = Computer.ClampMin(value, SpectrumViewViewModel.LineMinimumHeight);
            }
        }

        /// <summary>
        /// The height of this line in percents.
        /// </summary>
        public double HeightPercent { get; set; }

        /// <summary>
        /// The margin of this line.
        /// </summary>
        public Thickness Margin { get; set; } = new Thickness();

        /// <summary>
        /// The type of this line.
        /// </summary>
        public SpectrumLineType LineType { get; set; } = SpectrumLineType.Inner;

        /// <summary>
        /// The color of this line
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.ForegroundDark;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumLineViewModel"/> class.
        /// </summary>
        /// <param name="heightPercent">The height percent of this line.</param>
        public SpectrumLineViewModel(double heightPercent)
        {
            HeightPercent = heightPercent;
        }

        #endregion
    }
}
