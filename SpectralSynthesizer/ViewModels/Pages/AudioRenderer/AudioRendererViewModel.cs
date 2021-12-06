using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.DataModels.Enums;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="AudioRendererControl"/> and <see cref="AudioRendererDetailControl"/>.
    /// </summary>
    public class AudioRendererViewModel : BaseViewModel, IUndoableRedoable<AudioRendererModel>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<AudioRendererModel> ModelSaved;

        #endregion

        #region Properties

        #region Model

        /// <summary>
        /// The model of the <see cref="InstrumentBuilderControl"/>.
        /// </summary>
        public AudioRendererModel Model
        {
            get { return IoC.Get<ProjectModel>().AudioRenderer; }
            set
            {
                IoC.Get<ProjectModel>().AudioRenderer.PropertyChanged -= OnPropertyChanged;
                IoC.Get<ProjectModel>().AudioRenderer.PropertyChanged += OnPropertyChanged;
            }
        }

        #endregion

        #region Instruments ComboBox

        /// <summary>
        /// The combobox of the <see cref="ProjectModel.Instruments"/>.
        /// </summary>
        public ComboBoxViewModel<Instrument> InstrumentsComboBox { get; } = new ComboBoxViewModel<Instrument>(IoC.Get<ProjectModel>().Instruments);

        /// <summary>
        /// The height of the instrument combobox.
        /// </summary>
        public double InstrumentsComboBoxHeight => 90;

        #endregion

        #region View ViewModels

        /// <summary>
        /// The <see cref="Midi"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel MidiRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The selected <see cref="Midi"/>'s view model.
        /// </summary>
        public MidiViewViewModel MidiViewViewModel { get; } = new MidiViewViewModel();

        /// <summary>
        /// The <see cref="Wave"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel WaveRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The selected <see cref="Wave"/>'s viewmodel.
        /// </summary>
        public WaveViewViewModel WaveViewViewModel { get; set; } = new WaveViewViewModel();

        #endregion

        #region Audio Buffer ViewModels

        /// <summary>
        /// The viewmodel of the loaded <see cref="Wave"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel WaveAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Midi"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel MidiAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        #endregion

        #region Audio Rendering

        /// <summary>
        /// Indicates whether the <see cref="Instrument"/> generation is enabled or not.
        /// </summary>
        public bool IsRenderingEnabled => Model.Instrument != null && MidiViewViewModel.IsContentLoaded;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the audio rendering parameters is open or not.
        /// </summary>
        public bool IsRenderingGroupBoxOpen { get; set; } = false;

        private bool _renderingSelectionOnly = false;

        /// <summary>
        /// Indicates whether the audio rendering process should only be related to the <see cref="Wave"/>'s selected part or not.
        /// </summary>
        public bool RenderingSelectionOnly
        {
            get { return _renderingSelectionOnly; }
            set
            {
                if (value != _renderingSelectionOnly)
                {
                    _renderingSelectionOnly = value;
                    SaveModelOnParametersChanged();
                }
            }
        }

        private bool _renderingEnableSustain = false;

        /// <summary>
        /// Indicates whether the audio rendering process should be rendered as all <see cref="Note"/>s are played until their original length.
        /// </summary>
        public bool RenderingEnableSustain
        {
            get { return _renderingEnableSustain; }
            set
            {
                if (value != _renderingEnableSustain)
                {
                    _renderingEnableSustain = value;
                    SaveModelOnParametersChanged();
                }
            }
        }

        /// <summary>
        /// The loading window of the instrument generation.
        /// </summary>
        public LoadingWindowViewModel AudioRenderingWindow { get; } = new LoadingWindowViewModel("rendering audio", AudioRendererModel.AudioRendererPercentManager);

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command for audio rendering.
        /// </summary>
        public ICommand RenderCommand { get; set; }

        #endregion

        #region Methods

        #region Load

        /// <summary>
        /// Loads the <see cref="AudioRendererModel"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        public void LoadModel(AudioRendererModel model)
        {
            UnloadAll();
            Model = model;
            InitBuffers();
            SetDefaultValues();
            if (Model.Midi != null)
            {
                LoadMidi();
            }
            if (Model.Wave != null)
            {
                LoadWave(Model.Wave);
            }
            if (Model.Instrument != null)
            {
                InstrumentsComboBox.SelectElement(Model.Instrument);
            }
        }

        /// <summary>
        /// Unloads this page.
        /// </summary>
        private void UnloadAll()
        {
            LoadMidi();
            WaveViewViewModel.LoadModel(null);
        }

        /// <summary>
        /// Loads the given <see cref="Midi"/> to the model and updates the view.
        /// </summary>
        /// <param name="filePath">The path to the midi.</param>
        public void LoadMidi(string filePath)
        {
            Model.LoadMidi(filePath);
            ModelSaved?.Invoke(Model);
            MidiViewViewModel.LoadModel(Model.Midi);
            MidiRulerViewViewModel.LoadData(Model.Midi);
            if (Model.MidiAudioBuffer.State == AudioBufferState.Selected || Model.MidiAudioBuffer.State == AudioBufferState.Playing)
            {
                Model.StartMidiAudioRendering();
            }
        }


        /// <summary>
        /// Loads the given <see cref="Midi"/> from the model.
        /// </summary>
        private void LoadMidi()
        {
            MidiViewViewModel.LoadModel(Model.Midi);
            MidiRulerViewViewModel.LoadData(Model.Midi);
            if (Model.MidiAudioBuffer.State == AudioBufferState.Selected || Model.MidiAudioBuffer.State == AudioBufferState.Playing)
            {
                Model.StartMidiAudioRendering();
            }
        }


        /// <summary>
        /// Loads the given <see cref="Wave"/> to the model and updates the view.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        public void LoadWave(Wave wave)
        {
            Model.LoadWave(wave);
            ModelSaved?.Invoke(Model);
            WaveViewViewModel.LoadModel(Model.Wave);
            WaveRulerViewViewModel.LoadData(Model.Wave);
        }

        /// <summary>
        /// Loads the given <see cref="Instrument"/> to the model and updates the view.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/> to load.</param>
        public void LoadInstrument(Instrument instrument)
        {
            Model.LoadInstrument(instrument);
            ModelSaved?.Invoke(Model);
            OnPropertyChanged("");
        }

        #endregion

        #region Audio Rendering

        /// <summary>
        /// Begins the audio rendering process.
        /// </summary>
        private void BeginAudioRendering()
        {
            Model.BeginAudioRendering(MidiViewViewModel.Selection);
            AudioRenderingWindow.Show();
        }

        /// <summary>
        /// Ends the audio rendering process.
        /// </summary>
        /// <param name="wave">The rendered audio as a <see cref="Wave"/>.</param>
        public void OnEndAudioRendering(Wave wave)
        {
            LoadWave(wave);
            AudioRenderingWindow.Close();
        }

        /// <summary>
        /// Cancels the audio rendering process.
        /// </summary>
        public void OnCancelAudioRendering()
        {
            AudioRenderingWindow.Close();
        }

        #endregion

        #region Audio Buffer Selection

        /// <summary>
        /// Selects the given <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        /// <param name="buffer">The <see cref="AudioBufferWaveProvider"/> to select.</param>
        private void SelectAudioBuffer(AudioBufferWaveProvider buffer)
        {
            if (buffer.State == AudioBufferState.Loaded)
            {
                WaveAudioBufferViewModel.Deselect();
                MidiAudioBufferViewModel.Deselect();
                buffer.State = AudioBufferState.Selected;
                IoC.Get<AudioPlayer>().SetNextBuffer(buffer);
            }
        }


        #endregion

        #region Saving

        /// <summary>
        /// Saves the model when the parameters have changed.
        /// </summary>
        public void SaveModelOnParametersChanged()
        {
            Model.RenderingSelectionOnly = RenderingSelectionOnly;
            Model.RenderingEnableSustain = RenderingEnableSustain;
            ModelSaved?.Invoke(Model);
        }

        #endregion

        #region Init

        /// <summary>
        /// Sets up all the commands this class uses.
        /// </summary>
        private void InitCommands()
        {
            RenderCommand = new RelayCommand(() => BeginAudioRendering());
            AudioRenderingWindow.LoadingCancelled += Computer.CancelAudioRenderingTask;
        }

        /// <summary>
        /// Initializes the views.
        /// </summary>
        private void InitViews()
        {
            TimeDomainViewModel.SetUpFollowers(MidiViewViewModel, MidiRulerViewViewModel);
            TimeDomainViewModel.SetUpFollowers(WaveViewViewModel, WaveRulerViewViewModel);
            InstrumentsComboBox.SelectionChanged += LoadInstrument;
        }

        /// <summary>
        /// Initializes the <see cref="AudioBufferBorderViewModel"/>s.
        /// </summary>
        private void InitBufferViewModels()
        {
            WaveAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            MidiAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            MidiAudioBufferViewModel.LeftClicked += (_) => Model.StartMidiAudioRendering();
            MidiViewViewModel.PropertyChanged += OnPropertyChanged;
            WaveViewViewModel.PropertyChanged += OnPropertyChanged;
            WaveViewViewModel.MouseMiddleButtonClicked += WaveAudioBufferViewModel.Select;
            MidiViewViewModel.MouseMiddleButtonClicked += MidiAudioBufferViewModel.Select;
        }

        /// <summary>
        /// Initializes the <see cref="AudioBufferWaveProvider"/>s.
        /// </summary>
        private void InitBuffers()
        {
            WaveAudioBufferViewModel.LoadModel(Model.WaveAudioBuffer);
            MidiAudioBufferViewModel.LoadModel(Model.MidiAudioBuffer);
            WaveViewViewModel.SelectionChanged += Model.WaveAudioBuffer.ChangeSelection;
            MidiViewViewModel.SelectionChanged += Model.MidiAudioBuffer.ChangeSelection;
        }


        /// <summary>
        /// Sets the default slider values.
        /// </summary>
        private void SetDefaultValues()
        {
            _renderingSelectionOnly = Model.RenderingSelectionOnly;
            _renderingEnableSustain = Model.RenderingEnableSustain;
            OnPropertyChanged("");
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRendererViewModel"/> class.
        /// </summary>
        public AudioRendererViewModel()
        {
            InitCommands();
            InitViews();
            InitBufferViewModels();
        }

        #endregion
    }
}
