using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of any line in the <see cref="SpectogramView"/>.
    /// </summary>
    public class SpectogramTrajectoryViewModel : BaseViewModel, IHoverable<SpectogramTrajectoryViewModel>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event MouseOverChangedDelegate<SpectogramTrajectoryViewModel> MouseOverChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="RatioPoint"/>s of this trajectory.
        /// </summary>
        public List<RatioPoint<SpectralUnit>> SpectralPoints { get; set; } = new List<RatioPoint<SpectralUnit>>();

        /// <summary>
        /// The points of this trajectory.
        /// </summary>
        public ObservableCollection<Point> Points { get; set; } = new ObservableCollection<Point>();

        /// <summary>
        /// The color of this trajectory.
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.ForegroundDark;

        private bool _isMouseOver = false;

        /// <summary>
        /// Indicates whether the mouse is over this trajectory or not.
        /// </summary>
        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                if (_isMouseOver != value)
                {
                    _isMouseOver = value;
                    MouseOverChanged?.Invoke(this);
                }
            }
        }

        #endregion

        #region Commands

        /// <inheritdoc/>
        public ICommand MouseEnterCommand { get; set; }

        /// <inheritdoc/>
        public ICommand MouseLeaveCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Refreshes the position of this trajectory by repositioning all of its <see cref="Points"/>.
        /// </summary>
        /// <param name="width">The width of the <see cref="SpectogramView"/> in pixels.</param>
        /// <param name="height">The height of the <see cref="SpectogramView"/> in pixels.</param>
        /// <param name="tonePerNote">The current number of tones visible per note.</param>
        public void Position(double width, double height, int tonePerNote)
        {
            Points.Clear();
            foreach (var sp in SpectralPoints)
            {
                Points.Add(new Point(sp.Position * width, height - (Computer.FrequencyToDiscreteFrequency(sp.Value.Frequency) / tonePerNote * SpectogramViewViewModel.SpectogramLineHeight + 1)));
            }
            OnPropertyChanged("");
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectogramTrajectoryViewModel"/> class.
        /// </summary>
        /// <param name="spectralPoints">The relevant <see cref="SpectralPoints"/> of the <see cref="SpectralTrajectory"/>.</param>
        public SpectogramTrajectoryViewModel(IList<RatioPoint<SpectralUnit>> spectralPoints)
        {
            SpectralPoints.Clear();
            SpectralPoints.AddRange(spectralPoints);
            MouseEnterCommand = new RelayCommand(() => IsMouseOver = true);
            MouseLeaveCommand = new RelayCommand(() => IsMouseOver = false);
        }

        #endregion
    }
}
