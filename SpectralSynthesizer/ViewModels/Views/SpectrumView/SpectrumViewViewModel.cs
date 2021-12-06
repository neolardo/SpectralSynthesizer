using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// THe viewmodel of the <see cref="SpectrumView"/>.
    /// </summary>
    public class SpectrumViewViewModel : ScrollableZoomableViewViewModel, IUndoableRedoable<Spectrum>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<Spectrum> ModelSaved;

        #endregion

        #region Properties

        #region Model

        private Spectrum _model;

        /// <summary>
        /// The model spectrum.
        /// </summary>
        public Spectrum Model
        {
            get { return _model; }
            set
            {
                if (_model != null)
                    _model.PropertyChanged -= OnPropertyChanged;
                _model = value;
                if (_model != null)
                    _model.PropertyChanged += OnPropertyChanged;
            }
        }

        #endregion

        /// <summary>
        /// The lines of the spectrum.
        /// </summary>
        public ObservableCollection<SpectrumLineViewModel> Lines { get; set; } = new ObservableCollection<SpectrumLineViewModel>();

        /// <summary>
        /// The label ruler.
        /// </summary>
        public ObservableCollection<LabelViewModel> Labels { get; set; } = new ObservableCollection<LabelViewModel>();

        /// <summary>
        /// Indicates that how many subnotes are contained in one visible note.
        /// The <see cref="ScrollableZoomableViewViewModel.PixelPerData"/> value is different from the <see cref="ScrollableZoomableViewViewModel"/> one.
        /// </summary>
        private int TonePerNote => ProjectModel.TonePerNote / (int)PixelPerData;

        /// <summary>
        /// The maximum width of the <see cref="LineWidth"/>.
        /// </summary>
        private static double LineMaximumWidth => 3.5;

        private double _lineWidth = 2;

        /// <summary>
        /// The width of any <see cref="SpectrumLineViewModel"/>.
        /// </summary>
        public double LineWidth
        {
            get { return _lineWidth; }
            set
            {
                _lineWidth = Computer.ClampMax(value, LineMaximumWidth);
            }
        }

        /// <summary>
        /// The margin width of a <see cref="SpectrumLineType.Inner"/> line.
        /// </summary>
        private double InnerMarginWidth => LineWidth / 2;

        /// <summary>
        /// The margin width of a <see cref="SpectrumLineType.Normal"/> line.
        /// </summary>
        private double NormalMarginWidth => LineWidth;

        /// <summary>
        /// The margin width of a <see cref="SpectrumLineType.Starter"/> line.
        /// </summary>
        private double StarterMarginWidth => LineWidth * 4;

        /// <summary>
        /// The minimum height of spectrum lines in pixels.
        /// </summary>
        public static double LineMinimumHeight => 2;

        /// <summary>
        /// The height of the label ruler area in pixels.
        /// </summary>
        public double LabelHeight => 14;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the model <see cref="Spectrum"/>.
        /// </summary>
        /// <param name="model">The spectrum.</param>
        public void LoadModel(Spectrum model)
        {
            Model = model;
            if (Model != null)
            {
                CalculateLengthAndParameters();
                ConvertModelToViewData();
                PositionLines();
                CreateLabels();
                PositionLabels();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Sets the length based on the <see cref="TonePerNote"/> value and calculates the parameters for the view.
        /// </summary>
        private void CalculateLengthAndParameters()
        {
            if (ScrollWidth > 0)
            {
                int minLineCount = ProjectModel.TotalNoteNumber;
                int starter = 12;
                double minContentLength = ScrollWidth;
                double spectrumLineUnit = minLineCount + minLineCount * (NormalMarginWidth / LineWidth) + (minLineCount / starter - 1) * ((StarterMarginWidth - NormalMarginWidth) / LineWidth);
                LineWidth = minContentLength / spectrumLineUnit;
            }
            int lineCount = ProjectModel.TotalNoteNumber;
            int normalWidth = (int)PixelPerData;
            int starterWidth = (int)PixelPerData * 12;
            double length = lineCount * (LineWidth + InnerMarginWidth);
            length += (lineCount / normalWidth) * (NormalMarginWidth - InnerMarginWidth);
            length += (lineCount / starterWidth - 1) * (StarterMarginWidth - NormalMarginWidth);
            Length = length;
        }


        /// <summary>
        /// Converts the loaded model into lines.
        /// </summary>
        private void ConvertModelToViewData()
        {
            if (Model == null)
                return;
            Lines.Clear();
            float[] amplitudes = Model.ToAmplitudeArray(TonePerNote);
            int starter = (int)PixelPerData * 12;
            int normal = (int)PixelPerData;
            for (int i = 0; i < amplitudes.Length; i++)
            {
                var line = new SpectrumLineViewModel(amplitudes[i]);
                if (i % starter == 0)
                {
                    line.LineType = SpectrumLineType.Starter;
                }
                else if (i % normal == 0)
                {
                    line.LineType = SpectrumLineType.Normal;
                }
                else
                {
                    line.LineType = SpectrumLineType.Inner;
                }
                Lines.Add(line);
            }
        }

        /// <summary>
        /// Refreshes the positions of the lines.
        /// </summary>
        private void PositionLines()
        {
            if (Model == null)
                return;
            // the first line does not have a margin
            Lines[0].Height = Lines[0].HeightPercent * Height;
            Lines[0].Width = LineWidth;
            for (int i = 1; i < Lines.Count; i++)
            {
                Lines[i].Height = Lines[i].HeightPercent * Height;
                Lines[i].Width = LineWidth;
                switch (Lines[i].LineType)
                {
                    case SpectrumLineType.Starter:
                        Lines[i].Margin = new Thickness(StarterMarginWidth, 0, 0, 0);
                        break;
                    case SpectrumLineType.Normal:
                        Lines[i].Margin = new Thickness(NormalMarginWidth, 0, 0, 0);
                        break;
                    case SpectrumLineType.Inner:
                        Lines[i].Margin = new Thickness(InnerMarginWidth, 0, 0, 0);
                        break;
                }
            }
        }

        /// <summary>
        /// Refreshes the <see cref="Lines"/>' heights.
        /// </summary>
        private void RefreshLineHeights()
        {
            foreach (var l in Lines)
            {
                l.Height = l.HeightPercent * Height;
            }
        }

        /// <summary>
        /// Creates the labels.
        /// </summary>
        private void CreateLabels()
        {
            if (Model == null)
                return;
            Labels.Clear();
            int octave = 0;
            double x = 0;
            foreach (SpectrumLineViewModel line in Lines)
            {
                x += line.Margin.Left;
                if (line.LineType == SpectrumLineType.Starter)
                {
                    Labels.Add(new LabelViewModel(octave.ToString(), x));
                    octave++;
                }
                x += line.Width;
                // 10.th octave is not needed
                if (octave == 10)
                    break;
            }
        }

        /// <summary>
        /// Positions the labels.
        /// </summary>
        private void PositionLabels()
        {
            int octave = 0;
            double x = 0;
            foreach (SpectrumLineViewModel line in Lines)
            {
                x += line.Margin.Left;
                if (line.LineType == SpectrumLineType.Starter)
                {
                    Labels[octave].X = x;
                    octave++;
                }
                x += line.Width;
                // 10.th octave is not needed
                if (octave == 10)
                    break;
            }
        }

        /// <inheritdoc/>
        protected override void RefreshOnScrollViewerSizeChanged()
        {
            Height = ScrollHeight;
            RefreshLineHeights();
            CalculateLengthAndParameters();
            PositionLines();
            PositionLabels();
        }


        /// <inheritdoc/>
        protected override void RefreshOnZoom()
        {
            CalculateLengthAndParameters();
            ConvertModelToViewData();
            PositionLines();
            PositionLabels();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumViewViewModel"/> class.
        /// </summary>
        public SpectrumViewViewModel()
        {
            SetUpZoomingParameters(1, ProjectModel.TonePerNote, 1, 2);
        }

        #endregion
    }
}
