using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The the viewmodel of the <see cref="WaveView"/>.
    /// </summary>
    public class WaveViewViewModel : OptimizedTimeDomainViewModel, IUndoableRedoable<Wave>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<Wave> ModelSaved;

        #endregion

        #region Properties

        #region Model

        private Wave _model;

        /// <summary>
        /// The wave model.
        /// </summary>
        public Wave Model
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
        /// Represents the lines in the wave.
        /// </summary>
        public ObservableCollection<WaveLineViewModel> Lines { get; set; } = new ObservableCollection<WaveLineViewModel>();

        /// <summary>
        /// The name of the audio file.
        /// </summary>
        public string FileName => Model == null ? "" : Model.FileName;

        #region Static

        /// <summary>
        /// The margin of one line.
        /// </summary>
        public static Thickness LineMargin => new Thickness(0, 0, 4, 0);

        /// <summary>
        /// The width of one line in pixels.
        /// </summary>
        public static double LineWidth => 2;

        /// <summary>
        /// The width of one line plus the margin.
        /// </summary>
        private static double LineWidthAndMargin => LineWidth + LineMargin.Left + LineMargin.Right;

        /// <summary>
        /// The minimum height of one line.
        /// </summary>
        public static double LineMinimumHeight => 2;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Loads the wave model
        /// </summary>
        /// <param name="model">The wave</param>
        public void LoadModel(Wave model)
        {
            Model = model;
            if (Model != null)
            {
                Length = Model.Length / 1000f * PixelPerSecond;
                SelectAll();
                ConvertDataToLines();
                RefreshOnSelectionChanged();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        #region Model to View Data Conversion

        /// <summary>
        /// Converts the loaded wave data into lines.
        /// </summary>
        private void ConvertDataToLines()
        {
            if (Model == null)
                return;
            int lineCount = (int)(ContentWidth / LineWidthAndMargin);
            AddOrDeleteLinesUntilLineCount(lineCount);
            int startIndex = (int)((ContentMargin.Left / Length) * Model.Data.Length);
            int endIndex = (int)(((ContentMargin.Left + ContentWidth) / Length) * Model.Data.Length);
            int dataPerLine = (int)((endIndex - startIndex) / Lines.Count);
            for (int i = 0; i < Lines.Count; i++)
            {
                int index = startIndex + i * dataPerLine;
                int searchStart = index - (index % dataPerLine);
                int searchEnd = searchStart + dataPerLine;
                float max = GetDataMaxAmp(searchStart, searchEnd);
                Lines[i].HeightPercent = max * 100.0;
                Lines[i].Height = Height * Lines[i].HeightPercent / 100.0;
            }
        }

        /// <summary>
        /// Adds or deletes <see cref="WaveLineViewModel"/>s until the given count is reached.
        /// </summary>
        /// <param name="lineCount">The goal number of <see cref="WaveLineViewModel"/>s.</param>
        private void AddOrDeleteLinesUntilLineCount(int lineCount)
        {
            while (Lines.Count > lineCount)
                Lines.RemoveAt(0);
            while (Lines.Count < lineCount)
            {
                var line = new WaveLineViewModel();
                line.RightClicked += OnWaveLineRightClicked;
                line.LeftClicked += OnWaveLineLeftClicked;
                Lines.Add(line);
            }
        }

        /// <summary>
        /// Gets the maximum amplitude of the loaded float data in the given range.
        /// </summary>
        /// <param name="startIndex">The start index of the range.</param>
        /// <param name="endIndex">The end index of the range.</param>
        /// <returns></returns>
        private float GetDataMaxAmp(int startIndex, int endIndex)
        {
            float max = Model.Data[startIndex];
            int i = startIndex + 1;
            while (i < Model.Data.Length && i < endIndex)
            {
                if (Math.Abs(Model.Data[i]) > max)
                    max = Math.Abs(Model.Data[i]);
                i++;
            }
            return max;
        }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void RefreshOnZoom()
        {
            if (Model == null)
                return;
            Length = Model.Length / 1000f * PixelPerSecond;
            ConvertDataToLines();
            RefreshOnSelectionChanged();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void RefreshOnScroll()
        {
            ConvertDataToLines();
            RefreshOnSelectionChanged();
        }

        /// <inheritdoc/>
        protected override void RefreshOnScrollViewerSizeChanged()
        {
            Height = ScrollHeight;
            ConvertDataToLines();
            RefreshOnSelectionChanged();
        }

        #region Selection

        /// <summary>
        /// Called on left clicking a <see cref="WaveLineViewModel"/>. 
        /// </summary>
        /// <param name="line">The line which was clicked.</param>
        public void OnWaveLineLeftClicked(WaveLineViewModel line) => SelectFrom((double)Lines.IndexOf(line) / Lines.Count * ContentWidth + ContentMargin.Left);

        /// <summary>
        /// Called on right clicking a <see cref="WaveLineViewModel"/>. 
        /// </summary>
        /// <param name="line">The line which was clicked.</param>
        public void OnWaveLineRightClicked(WaveLineViewModel line) => SelectTo((double)Lines.IndexOf(line) / Lines.Count * ContentWidth + ContentMargin.Left);

        /// <summary>
        /// Refreshes the selection color of the lines.
        /// </summary>
        protected override void RefreshOnSelectionChanged()
        {
            int selectionMinInd = (int)(((Selection.Start * Length) - ContentMargin.Left) / LineWidthAndMargin);
            int selectionMaxInd = (int)(((Selection.End * Length) - ContentMargin.Left) / LineWidthAndMargin);
            for (int i = 0; i < Lines.Count; i++)
            {
                //to make it refresh
                Lines[i].Color = ApplicationColor.BackgroundDark;

                if (i >= selectionMinInd && i <= selectionMaxInd)
                    Lines[i].Color = ApplicationColor.Theme;
                else
                    Lines[i].Color = ApplicationColor.BackgroundLight;
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveViewViewModel"/> class.
        /// </summary>
        public WaveViewViewModel()
        {
            IoC.Get<ProjectModel>().GeneralSettings.ThemeChanged += RefreshOnSelectionChanged;
        }

        #endregion
    }
}
