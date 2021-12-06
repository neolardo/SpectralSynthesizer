using SpectralSynthesizer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents a wave line's viewmodel
    /// </summary>
    public class WaveLineViewModel : BaseViewModel, IRightClickable<WaveLineViewModel>, ILeftClickable<WaveLineViewModel>
    {
        #region Delegates and events

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<WaveLineViewModel> RightClicked;

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<WaveLineViewModel> LeftClicked;

        #endregion

        #region Properties

        /// <summary>
        /// The height of this line in percents.
        /// </summary>
        public double HeightPercent { get; set; } = 0;

        private double _height = WaveViewViewModel.LineMinimumHeight;

        /// <summary>
        /// The height of this line in pixels.
        /// </summary>
        public double Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = Computer.ClampMin(value, WaveViewViewModel.LineMinimumHeight);
            }
        }

        /// <summary>
        /// The color of this line.
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.BackgroundLight;

        /// <summary>
        /// The width of this line.
        /// </summary>
        public double Width => WaveViewViewModel.LineWidth;

        /// <summary>
        /// The margin of this line.
        /// </summary>
        public Thickness Margin => WaveViewViewModel.LineMargin;

        #endregion

        #region Commands

        /// <inheritdoc/>
        public ICommand RightClickCommand { get; set; }

        /// <inheritdoc/>
        public ICommand LeftClickCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveLineViewModel"/> class.
        /// </summary>
        public WaveLineViewModel()
        {
            RightClickCommand = new RelayCommand(() => RightClicked?.Invoke(this));
            LeftClickCommand = new RelayCommand(() => LeftClicked?.Invoke(this));
        }

        #endregion
    }
}
