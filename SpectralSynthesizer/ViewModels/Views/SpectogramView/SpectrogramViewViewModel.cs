using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="SpectogramView"/>.
    /// </summary>
    public class SpectogramViewViewModel : TimeDomainViewModel, IUndoableRedoable<Note>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<Note> ModelSaved;

        /// <summary>
        /// Delegate for <see cref="TimeDomainSpectrum"/> selection events.
        /// </summary>
        /// <param name="spectrum">The <see cref="TimeDomainSpectrum"/> which has been selected.</param>
        public delegate void SelectedSpectrumChangedDelegate(TimeDomainSpectrum timeDomainSpectrum);

        /// <summary>
        /// Fires off when a <see cref="TimeDomainSpectrum"/> of the loaded <see cref="Note"/> has been selected.
        /// </summary>
        public event SelectedSpectrumChangedDelegate SelectedSpectrumChanged;

        #endregion

        #region Properties

        #region Model

        private Note _model;

        /// <summary>
        /// The model note.
        /// </summary>
        public Note Model
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
        /// The list of <see cref="SpectogramTrajectoryViewModel"/>s.
        /// </summary>
        public ObservableCollection<SpectogramTrajectoryViewModel> Trajectories { get; set; } = new ObservableCollection<SpectogramTrajectoryViewModel>();

        /// <summary>
        /// The list of <see cref="FrequencyRulerItemViewModel"/>s.
        /// </summary>
        public ObservableCollection<FrequencyRulerItemViewModel> FrequencyRulerItems { get; set; } = new ObservableCollection<FrequencyRulerItemViewModel>();

        /// <summary>
        /// Indicates whether the user can interract with the selected notes or not.
        /// </summary>
        public bool IsSelectionInteractable { get; set; } = true;

        /// <summary>
        /// The height of any frequency ruler item.
        /// </summary>
        public double FrequencyRulerItemHeight { get; set; }

        #region Static

        /// <summary>
        /// The inner thickness of any <see cref="SpectogramTrajectoryViewModel"/>.
        /// </summary>
        public static double SpectogramLineInnerThickness => 2;

        /// <summary>
        /// The space between any <see cref="SpectogramTrajectoryViewModel"/>.
        /// </summary>
        public static double SpectogramLineSpace => 4;

        /// <summary>
        /// The height of the <see cref="SpectogramTrajectoryViewModel"/>.
        /// </summary>
        public static double SpectogramLineHeight => SpectogramLineInnerThickness + SpectogramLineSpace;

        /// <summary>
        /// The outer thickness of any <see cref="SpectogramTrajectoryViewModel"/>.
        /// </summary>
        public static double SpectogramLineOuterThickness => SpectogramLineInnerThickness + SpectogramLineSpace * 2;

        /// <summary>
        /// The width of the frequency ruler.
        /// </summary>
        public static double FrequencyRulerWidth => 30;

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command for right clicking of the content.
        /// </summary>
        public ICommand ContentRightClickCommand { get; set; }

        /// <summary>
        /// Command for left clicking on the content.
        /// </summary>
        public ICommand ContentLeftClickCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the <see cref="Note"/> model.
        /// </summary>
        /// <param name="midi">The <see cref="Note"/>.</param>
        public void LoadModel(Note model)
        {
            Model = model;
            if (Model != null)
            {
                ConvertModelToViewModel();
                RefreshLocations();
                ColorizeLines();
                SelectAll();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Converts the midi to view model data.
        /// </summary>
        private void ConvertModelToViewModel()
        {
            Trajectories.Clear();
            var necessaryPoints = new List<RatioPoint<SpectralUnit>>();
            foreach (var trajectory in Model.Sinusoid.Trajectories)
            {
                if (trajectory.SpectralPoints.Count > 0)
                {
                    var trajectoryVM = new SpectogramTrajectoryViewModel(trajectory.SpectralPoints);
                    trajectoryVM.MouseOverChanged += OnMouseOverLineChanged;
                    Trajectories.Add(trajectoryVM);
                }
            }
        }

        /// <summary>
        /// Refreshes the locations of the lines.
        /// </summary>
        public void RefreshLocations()
        {
            if (Trajectories == null || Model == null)
                return;
            Length = Model.Length / 1000.0 * PixelPerSecond;
            foreach (var trajectory in Trajectories)
            {
                trajectory.Position(Length, Height, ProjectModel.TonePerNote / (int)AltPixelPerData);
            }
        }

        /// <summary>
        /// Called when a <see cref="SpectogramTrajectoryViewModel"/>'s mouse over property has changed.
        /// </summary>
        /// <param name="trajectory">The line.</param>
        private void OnMouseOverLineChanged(SpectogramTrajectoryViewModel trajectory)
        {
            double selectionStart = Selection.Start * Length;
            double selectionEnd = Selection.End * Length;
            if (trajectory.IsMouseOver)
            {
                trajectory.Color = ApplicationColor.ForegroundLight;
            }
            else
            {
                trajectory.Color = ApplicationColor.Theme;
            }
        }

        /// <summary>
        /// Colorizes the lines.
        /// </summary>
        public void ColorizeLines()
        {
            if (Trajectories == null || Trajectories.Count == 0)
                return;
            foreach (SpectogramTrajectoryViewModel trajectory in Trajectories)
            {
                trajectory.Color = ApplicationColor.BackgroundDark; // to refresh
                trajectory.Color = trajectory.IsMouseOver ? ApplicationColor.ForegroundLight : ApplicationColor.Theme;
            }
        }

        /// <summary>
        /// Refreshes the height parameters of this view.
        /// </summary>
        private void RefreshHeights()
        {
            Height = ProjectModel.TotalDiscreteFrequencyNumber / (int)(ProjectModel.TonePerNote / AltPixelPerData) * SpectogramLineHeight;
            FrequencyRulerItemHeight = AltPixelPerData * SpectogramLineHeight;
            if (AltPixelPerData < 4)
                FrequencyRulerItemHeight *= 2;
            if (AltPixelPerData < 2)
                FrequencyRulerItemHeight *= 2;
        }

        /// <inheritdoc/>
        protected override void RefreshOnZoom()
        {
            RefreshLocations();
            ColorizeLines();
        }

        /// <inheritdoc/>
        protected override void RefreshOnAltZoom()
        {
            RefreshHeights();
            CreateFrequencyRuler();
            RefreshLocations();
        }

        #region Mouse Click

        /// <summary>
        /// Called on content left click.
        /// </summary>
        /// <param name="parameter">the content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentLeftClicked(object parameter)
        {
            var grid = parameter as Grid;
            SelectFrom(Mouse.GetPosition(grid).X);
        }

        /// <summary>
        /// Called on content right click.
        /// </summary>
        /// <param name="parameter">the content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentRightClicked(object parameter)
        {
            var grid = parameter as Grid;
            SelectTo(Mouse.GetPosition(grid).X);
        }

        #endregion

        #region Selection

        /// <inheritdoc/>
        protected override void RefreshOnSelectionChanged() => ColorizeLines();

        #endregion

        #region Frequency Ruler

        /// <summary>
        /// Creates the frequency ruler.
        /// </summary>
        public void CreateFrequencyRuler()
        {
            FrequencyRulerItems.Clear();
            int plus = ProjectModel.TonePerNote;
            if (AltPixelPerData < 4)
                plus *= 2;
            if (AltPixelPerData < 2)
                plus *= 2;
            for (int i = 0; i < ProjectModel.TotalDiscreteFrequencyNumber; i += plus)
            {
                var item = new FrequencyRulerItemViewModel((i / ProjectModel.TonePerNote).ToString());
                FrequencyRulerItems.Insert(0, item);
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectogramViewViewModel"/> class.
        /// </summary>
        public SpectogramViewViewModel()
        {
            CreateFrequencyRuler();
            RefreshHeights();
            IoC.Get<ProjectModel>().GeneralSettings.ThemeChanged += ColorizeLines;
            ContentLeftClickCommand = new ParameterizedRelayCommand((param) => OnContentLeftClicked(param));
            ContentRightClickCommand = new ParameterizedRelayCommand((param) => OnContentRightClicked(param));
        }

        #endregion
    }
}