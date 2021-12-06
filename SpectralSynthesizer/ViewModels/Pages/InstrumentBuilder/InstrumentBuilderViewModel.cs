using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.DataModels.Enums;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="InstrumentBuilderControl"/> and <see cref="InstrumentBuilderDetailControl"/>.
    /// </summary>
    public class InstrumentBuilderViewModel : BaseViewModel, IUndoableRedoable<InstrumentBuilderModel>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<InstrumentBuilderModel> ModelSaved;

        #endregion

        #region Properties

        #region Model

        /// <summary>
        /// The model of the <see cref="InstrumentBuilderControl"/>.
        /// </summary>
        public InstrumentBuilderModel Model
        {
            get { return IoC.Get<ProjectModel>().InstrumentBuilder; }
            set
            {
                IoC.Get<ProjectModel>().InstrumentBuilder.PropertyChanged -= OnPropertyChanged;
                IoC.Get<ProjectModel>().InstrumentBuilder.PropertyChanged += OnPropertyChanged;
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
        /// The <see cref="Wave"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel WaveRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The selected <see cref="Wave"/>'s viewmodel.
        /// </summary>
        public WaveViewViewModel WaveViewViewModel { get; set; } = new WaveViewViewModel();

        /// <summary>
        /// The selected single <see cref="Midi"/>'s view model.
        /// </summary>
        public SingleMidiViewViewModel SingleMidiViewViewModel { get; } = new SingleMidiViewViewModel();

        /// <summary>
        /// The <see cref="Sinusoid"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel SinusoidRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The <see cref="Sinusoid"/> viewmodel of the selected <see cref="Note"/>.
        /// </summary>
        public SpectogramViewViewModel SinusoidViewViewModel { get; } = new SpectogramViewViewModel();

        /// <summary>
        /// The <see cref="Transient"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel TransientRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The <see cref="Transient"/> viewmodel of the selected <see cref="Note"/>.
        /// </summary>
        public WaveViewViewModel TransientViewViewModel { get; } = new WaveViewViewModel();

        /// <summary>
        /// The <see cref="Noise"/> <see cref="RulerView"/>'s viewmodel.
        /// </summary>
        public RulerViewViewModel NoiseRulerViewViewModel { get; } = new RulerViewViewModel();

        /// <summary>
        /// The <see cref="Noise"/> viewmodel of the selected <see cref="Note"/>.
        /// </summary>
        public WaveViewViewModel NoiseViewViewModel { get; } = new WaveViewViewModel();

        /// <summary>
        /// The selected <see cref="Instrument"/> viewmodel.
        /// </summary>
        public InstrumentViewViewModel InstrumentViewViewModel { get; } = new InstrumentViewViewModel();


        #endregion

        #region Audio Buffer ViewModels

        /// <summary>
        /// The viewmodel of the loaded <see cref="Wave"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel WaveAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Midi"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel SingleMidiAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Instrument"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel InstrumentAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Note"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel NoteAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Note"/>'s <see cref="Sinusoid"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel SinusoidAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Note"/>'s <see cref="Transient"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel TransientAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        /// <summary>
        /// The viewmodel of the loaded <see cref="Note"/>'s <see cref="Noise"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferBorderViewModel NoiseAudioBufferViewModel { get; } = new AudioBufferBorderViewModel();

        #endregion

        #region Wave to Midi Conversion

        /// <summary>
        /// Indicates whether the <see cref="Wave"/> to <see cref="Midi"/> conversion is enabled or not.
        /// </summary>
        public bool IsWaveToMidiConversionEnabled => WaveViewViewModel.IsContentLoaded;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the <see cref="Wave"/> to <see cref="Midi"/> conversion's parameters is open or not.
        /// </summary>
        public bool IsWaveToMidiGroupBoxOpen { get; set; } = false;

        /// <summary>
        /// The <see cref="LoadingStatus"/> of the <see cref="Wave"/> to <see cref="Midi"/> conversion. 
        /// </summary>
        public LoadingStatus WaveToMidiConversionStatus { get; set; } = LoadingStatus.Empty;

        private bool _conversionSelectionOnly = false;

        /// <summary>
        /// Indicates whether the conversion should only be applied on the wave selected area or not.
        /// </summary>
        public bool ConversionSelectionOnly
        {
            get { return _conversionSelectionOnly; }
            set
            {
                if (value != _conversionSelectionOnly)
                {
                    _conversionSelectionOnly = value;
                    SaveModelOnParametersChanged();
                }
            }
        }


        /// <summary>
        /// The minimum decibel amplitude of any possible note to be noticed in the <see cref="Wave"/> to <see cref="Midi"/> conversion process.
        /// </summary>
        public ParameterViewModel<float> ConversionMinimumDecibelAmplitude { get; } = new ParameterViewModel<float>();

        /// <summary>
        /// The minimum length of any possible note in milliseconds in the <see cref="Wave"/> to <see cref="Midi"/> conversion process.
        /// </summary>
        public ParameterViewModel<int> ConversionMinimumLength { get; } = new ParameterViewModel<int>();

        /// <summary>
        ///The maximum amount of silence in any possible note in milliseconds in the <see cref="Wave"/> to <see cref="Midi"/> conversion process.
        /// </summary>
        public ParameterViewModel<int> ConversionMaximumSilence { get; } = new ParameterViewModel<int>();

        #endregion

        #region Instrument Generation

        /// <summary>
        /// Indicates whether the <see cref="Instrument"/> generation is enabled or not.
        /// </summary>
        public bool IsInstrumentGenerationEnabled => Model.Instrument != null && SingleMidiViewViewModel.IsContentLoaded && WaveViewViewModel.IsContentLoaded;

        /// <summary>
        /// Indicates whether the <see cref="Instrument"/> clearing is enabled or not.
        /// </summary>
        public bool IsInstrumentClearingEnabled => Model.Instrument != null;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the <see cref="Instrument"/> generation's parameters is open or not.
        /// </summary>
        public bool IsInstrumentGenerationGroupBoxOpen { get; set; } = false;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the <see cref="Instrument"/> generation's <see cref="Sinusoid"/> model generation parameters is open or not.
        /// </summary>
        public bool IsInstrumentGenerationSinusoidGroupBoxOpen { get; set; } = false;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the <see cref="Instrument"/> generation's <see cref="Transient"/> model generation parameters is open or not.
        /// </summary>
        public bool IsInstrumentGenerationTransientGroupBoxOpen { get; set; } = false;

        /// <summary>
        /// Inidicates whether the <see cref="GroupBox"/> containing the <see cref="Instrument"/> generation's <see cref="Noise"/> model generation parameters is open or not.
        /// </summary>
        public bool IsInstrumentGenerationNoiseGroupBoxOpen { get; set; } = false;

        private bool _isNoteOverwritingEnabled = false;

        /// <summary>
        /// Indicates whether the <see cref="Note"/>s should be overwritten when updating the selected <see cref="Instrument"/>.
        /// </summary>
        public bool IsNoteOverwritingEnabled
        {
            get { return _isNoteOverwritingEnabled; }
            set
            {
                if (value != _isNoteOverwritingEnabled)
                {
                    _isNoteOverwritingEnabled = value;
                    SaveModelOnParametersChanged();
                }
            }
        }

        /// <summary>
        /// The loading window of the instrument generation.
        /// </summary>
        public LoadingWindowViewModel InstrumentGenerationWindow { get; } = new LoadingWindowViewModel("generating instrument", Instrument.InstrumentGenerationPercentManager);

        #region Sinusoid

        private bool _areFrequenciesFixed = false;

        /// <summary>
        /// Indicates whether the frequencies of the <see cref="Sinusoid"/> are fixed or not.
        /// </summary>
        public bool AreFrequenciesFixed
        {
            get { return _areFrequenciesFixed; }
            set
            {
                if (value != _areFrequenciesFixed)
                {
                    _areFrequenciesFixed = value;
                    SaveModelOnParametersChanged();
                }
            }
        }

        /// <summary>
        /// The minimum decibel amplitude value of any <see cref="SpectralUnit"/> in the <see cref="Sinusoid"/> generation process.
        /// </summary>
        public ParameterViewModel<float> SinusoidMinimumDecibelAmplitude { get; } = new ParameterViewModel<float>();

        /// <summary>
        /// The maximum amount of milliseconds a <see cref="SpectralTrajectory"/> can sleep in the <see cref="Sinusoid"/> generation process.
        /// </summary>
        public ParameterViewModel<int> SinusoidMaximumSleepingTime { get; } = new ParameterViewModel<int>();

        /// <summary>
        /// The minimum length of any <see cref="SpectralTrajectory"/> in milliseconds in the <see cref="Sinusoid"/> generation process.
        /// </summary>
        public ParameterViewModel<int> SinusoidMinimumLength { get; } = new ParameterViewModel<int>();

        /// <summary>
        /// The maximum range in base 12 logarithmic values where two <see cref="SpectralUnit"/>s are connected in the <see cref="Sinusoid"/> generation process.
        /// </summary>
        public ParameterViewModel<float> SinusoidContinuationRange { get; } = new ParameterViewModel<float>();

        #endregion

        #region Transient

        private bool _transientIsTransposable = false;

        /// <summary>
        /// Indicates whether the <see cref="Transient"/> is transposable.
        /// </summary>
        public bool TransientIsTransposable
        {
            get { return _transientIsTransposable; }
            set
            {
                if (value != _transientIsTransposable)
                {
                    _transientIsTransposable = value;
                    SaveModelOnParametersChanged();
                }
            }
        }

        /// <summary>
        /// The minimum strength of a detectable transient used in the <see cref="Transient"/> generation process.
        /// </summary>
        public ParameterViewModel<float> TransientStrength { get; } = new ParameterViewModel<float>();

        /// <summary>
        /// The number of adjacent fft bins used in the <see cref="Transient"/> generation process.
        /// </summary>
        public ParameterViewModel<int> TransientAdjacencyNumber { get; } = new ParameterViewModel<int>();

        /// <summary>
        /// The minimum ratio of fft bins needed to flag a sample as a transient sample in the <see cref="Transient"/> generation process.
        /// </summary>
        public ParameterViewModel<float> TransientFlagRatio { get; } = new ParameterViewModel<float>();

        #endregion

        #region Noise

        /// <summary>
        /// The sampling frequency used in the <see cref="Noise"/> generation process.
        /// </summary>
        public ParameterViewModel<float> NoiseSamplingFrequency { get; } = new ParameterViewModel<float>();

        #endregion

        #endregion

        #region Note Generation

        /// <summary>
        /// The <see cref="LoadingStatus"/> of the <see cref="Note"/> generation process.
        /// </summary>
        public LoadingStatus NoteGenerationStatus { get; set; } = LoadingStatus.Empty;

        /// <summary>
        /// Indicates whether a <see cref="Note"/> is selected or not.
        /// </summary>
        public bool IsNoteContentLoaded => Model.Note != null && NoteGenerationStatus == LoadingStatus.Loaded;


        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command for converting the loaded <see cref="Wave"/> file to <see cref="Midi"/>.
        /// </summary>
        public ICommand ConvertWaveToMidiCommand { get; set; }

        /// <summary>
        /// Command for clearing the loaded <see cref="Midi"/>.
        /// </summary>
        public ICommand ClearMidiCommand { get; set; }

        /// <summary>
        /// Adds a new <see cref="Instrument"/> to the <see cref="ProjectModel.Instruments"/> list.
        /// </summary>
        public ICommand AddInstrumentCommand { get; set; }

        /// <summary>
        /// Removes the loaded <see cref="Instrument"/> from the <see cref="ProjectModel.Instruments"/> list.
        /// </summary>
        public ICommand RemoveInstrumentCommand { get; set; }

        /// <summary>
        /// Command for updating the loaded <see cref="Instrument"/>.
        /// </summary>
        public ICommand UpdateInstrumentCommand { get; set; }

        /// <summary>
        /// Command for clearing the loaded <see cref="Instrument"/>.
        /// </summary>
        public ICommand ClearInstrumentCommand { get; set; }

        #endregion

        #region Methods

        #region Load

        /// <summary>
        /// Loads the <see cref="InstrumentBuilderModel"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        public void LoadModel(InstrumentBuilderModel model)
        {
            UnloadAll();
            Model = model;
            InitBuffers();
            SetDefaultValues();
            if (Model.Wave != null)
            {
                LoadWave();
            }
            if (Model.Instrument != null)
            {
                var note = Model.Note;
                InstrumentsComboBox.SelectElement(Model.Instrument);
                if (note != null)
                {
                    LoadNote(note);
                }
            }
        }

        /// <summary>
        /// Unloads this page.
        /// </summary>
        private void UnloadAll()
        {
            LoadWave();
            InstrumentViewViewModel.LoadModel(null);
            SinusoidViewViewModel.LoadModel(null);
            TransientViewViewModel.LoadModel(null);
            NoiseViewViewModel.LoadModel(null);
        }

        /// <summary>
        /// Loads the given <see cref="Wave"/> to the model and updates the view.
        /// </summary>
        /// <param name="filePath">The path to the wave.</param>
        public void LoadWave(string filePath)
        {
            Model.LoadWave(filePath);
            ModelSaved?.Invoke(Model);
            WaveViewViewModel.LoadModel(Model.Wave);
            WaveRulerViewViewModel.LoadData(Model.Wave);
            SingleMidiViewViewModel.InitFromTimeDomainModel(Model.Wave);
        }

        /// <summary>
        /// Loads the given <see cref="Wave"/> to the view from the model.
        /// </summary>
        private void LoadWave()
        {
            WaveViewViewModel.LoadModel(Model.Wave);
            WaveRulerViewViewModel.LoadData(Model.Wave);
            SingleMidiViewViewModel.InitFromTimeDomainModel(Model.Wave);
        }

        /// <summary>
        /// Loads the given <see cref="Midi"/> to the model and updates the view.
        /// </summary>
        /// <param name="filePath">The path to the midi.</param>
        public void LoadMidi(string filePath)
        {
            if (WaveViewViewModel.IsContentLoaded)
            {
                WaveToMidiConversionStatus = LoadingStatus.Loaded;
                Model.LoadMidi(filePath);
                ModelSaved?.Invoke(Model);
                SingleMidiViewViewModel.LoadModel(Model.Midi);
                WaveRulerViewViewModel.LoadData(Model.Midi);
            }
        }

        /// <summary>
        /// Loads the given <see cref="Instrument"/> to the model and updates the view.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/> to load.</param>
        public void LoadInstrument(Instrument instrument)
        {
            Model.LoadInstrument(instrument);
            ModelSaved?.Invoke(Model);
            InstrumentViewViewModel.LoadModel(Model.Instrument);
            OnPropertyChanged("");
        }

        /// <summary>
        /// Loads the given <see cref="Note"/> to the model and updates the view.
        /// </summary>
        /// <param name="note">The <see cref="Note"/> to load.</param>
        public void LoadNote(Note note)
        {
            Model.LoadNote(note);
            ModelSaved?.Invoke(Model);
            if (Model.NoteAudioBuffer.State == AudioBufferState.Selected || Model.NoteAudioBuffer.State == AudioBufferState.Playing)
            {
                Model.StartNoteAudioRendering();
            }
            if (note.Sinusoid != null)
            {
                SinusoidViewViewModel.LoadModel(Model.Note);
                SinusoidRulerViewViewModel.LoadData(Model.Note);
                if (Model.SinusoidAudioBuffer.State == AudioBufferState.Selected || Model.SinusoidAudioBuffer.State == AudioBufferState.Playing)
                {
                    Model.StartSinusoidAudioRendering();
                }
            }
            else
            {
                SinusoidViewViewModel.LoadModel(null);
            }
            if (note.Transient != null)
            {
                TransientViewViewModel.LoadModel(Model.Note.Transient.Wave);
                TransientRulerViewViewModel.LoadData(Model.Note.Transient.Wave);
            }
            else
            {
                TransientViewViewModel.LoadModel(null);
            }
            if (note.Noise != null)
            {
                NoiseViewViewModel.LoadModel(Model.Note.Noise.Wave);
                NoiseRulerViewViewModel.LoadData(Model.Note.Noise.Wave);
            }
            else
            {
                NoiseViewViewModel.LoadModel(null);
            }
            OnPropertyChanged("");
        }

        #endregion

        #region Wave to Midi Conversion

        /// <summary>
        /// Begins the <see cref="Wave"/> to <see cref="Midi"/> conversion.
        /// </summary>
        private void BeginWaveToMidiConversion()
        {
            SingleMidiViewViewModel.IsInteractable = false;
            ClearMidi();
            (double start, double end) selection = Model.ConversionSelectionOnly ? (WaveViewViewModel.Selection.Start, WaveViewViewModel.Selection.End) : (0, 1);
            Model.BeginWaveToMidiConversion(selection);
            WaveToMidiConversionStatus = LoadingStatus.Loading;
        }

        /// <summary>
        /// Called on the end of the <see cref="Wave"/> to <see cref="Midi"/> conversion.
        /// </summary>
        /// <param name="midi">The converted <see cref="Midi"/> result.</param>
        public void OnEndWaveToMidiConversion(Midi midi)
        {
            Model.EndWaveToMidiConversion(midi);
            ModelSaved?.Invoke(Model);
            SingleMidiViewViewModel.LoadModel(Model.Midi);
            WaveRulerViewViewModel.LoadData(Model.Midi);
            Model.MidiAudioBuffer.ChangeSelection(SingleMidiViewViewModel.Selection.Start, SingleMidiViewViewModel.Selection.End);
            WaveToMidiConversionStatus = LoadingStatus.Loaded;
            SingleMidiViewViewModel.IsInteractable = true;
        }

        /// <summary>
        /// Called on cancelling the conversion from the loaded <see cref="Wave"/> to <see cref="Midi"/>.
        /// </summary>
        public void OnCancelWaveToMidiConversion()
        {
            SingleMidiViewViewModel.IsInteractable = true;
            WaveToMidiConversionStatus = LoadingStatus.Empty;
        }

        /// <summary>
        /// Clears the loaded midi.
        /// </summary>
        private void ClearMidi()
        {
            Model.UnloadMidi();
            ModelSaved?.Invoke(Model);
            SingleMidiViewViewModel.UnloadModel();
            if (SingleMidiAudioBufferViewModel.State == AudioBufferState.Selected || SingleMidiAudioBufferViewModel.State == AudioBufferState.Playing)
            {
                IoC.Get<AudioPlayer>().SetNextBuffer(null);
            }
            WaveToMidiConversionStatus = LoadingStatus.Empty;
        }

        #endregion

        #region Instrument Generation

        /// <summary>
        /// Adds a new <see cref="Instrument"/> to the <see cref="ProjectModel.Instruments"/> list and loads.
        /// </summary>
        private void AddInstrument()
        {
            Model.AddInstrument();
            if (Model.Instrument == null)
            {
                InstrumentsComboBox.SelectElement(IoC.Get<ProjectModel>().Instruments[0]);
            }
        }

        /// <summary>
        /// Removes the loaded <see cref="Instrument"/> from the <see cref="ProjectModel.Instruments"/> list and unloads it.
        /// </summary>
        private void RemoveInstrument()
        {
            if (Model.Instrument != null)
            {
                Model.RemoveInstrument();
                if (IoC.Get<ProjectModel>().Instruments.Count > 0)
                {
                    InstrumentsComboBox.SelectElement(IoC.Get<ProjectModel>().Instruments[0]);
                }
                else
                {
                    LoadInstrument(null);
                }
            }
        }

        /// <summary>
        /// Begins the <see cref="Instrument"/> generation.
        /// </summary>
        private void BeginInstrumentGeneration()
        {
            var type = IsInstrumentGenerationSinusoidGroupBoxOpen ? InstrumentModelType.Sinusoid : 0;
            type |= IsInstrumentGenerationTransientGroupBoxOpen ? InstrumentModelType.Transient : 0;
            type |= IsInstrumentGenerationNoiseGroupBoxOpen ? InstrumentModelType.Noise : 0;
            Model.BeginInstrumentGeneration(WaveViewViewModel.Selection, type);
            InstrumentGenerationWindow.Show();
        }

        /// <summary>
        /// Ends the <see cref="Instrument"/> generation.
        /// </summary>
        /// <param name="instrument">The generated <see cref="Instrument"/>.</param>
        public void OnEndInstrumentGeneration(Instrument instrument)
        {
            if (Model.Instrument != null)
            {
                foreach (var n in instrument.Notes)
                {
                    Model.Instrument.AddNote(n, Model.IsNoteOverwritingEnabled);
                }
                Model.Instrument.ModelType = instrument.ModelType;
                LoadInstrument(Model.Instrument);
            }
            InstrumentGenerationWindow.Close();
        }

        /// <summary>
        /// Cancels the <see cref="Instrument"/> generation.
        /// </summary>
        public void OnCancelInstrumentGeneration()
        {
            InstrumentGenerationWindow.Close();
        }

        /// <summary>
        /// Clears the selected <see cref="Instrument"/>.
        /// </summary>
        private void ClearInstrument()
        {
            Model.ClearInstrument();
            ModelSaved?.Invoke(Model);
            InstrumentViewViewModel.LoadModel(Model.Instrument);
            OnPropertyChanged("");
        }

        #endregion

        #region Note Generation

        /// <summary>
        /// Begins the <see cref="Note"/> generation process.
        /// </summary>
        /// <param name="noteNumber">The note number of the new <see cref="Note"/>.</param>
        private void BeginNoteGeneration(int noteNumber)
        {
            if (Model.Instrument != null)
            {
                NoteGenerationStatus = LoadingStatus.Loading;
                Model.BeginNoteGeneration(noteNumber);
            }
        }

        /// <summary>
        /// Ends the <see cref="Note"/> generation process.
        /// </summary>
        /// <param name="note">The generated <see cref="Note"/>.</param>
        public void OnEndNoteGeneration(Note note)
        {
            if (Model.Instrument != null)
            {
                LoadNote(note);
                NoteGenerationStatus = LoadingStatus.Loaded;
            }
            else
            {
                NoteGenerationStatus = LoadingStatus.Empty;
            }
        }

        /// <summary>
        /// Cancels the <see cref="Note"/> generation process.
        /// </summary>
        public void OnCancelNoteGeneration()
        {
            NoteGenerationStatus = LoadingStatus.Empty;
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
                SingleMidiAudioBufferViewModel.Deselect();
                InstrumentAudioBufferViewModel.Deselect();
                NoteAudioBufferViewModel.Deselect();
                SinusoidAudioBufferViewModel.Deselect();
                TransientAudioBufferViewModel.Deselect();
                NoiseAudioBufferViewModel.Deselect();
                buffer.State = AudioBufferState.Selected;
                IoC.Get<AudioPlayer>().SetNextBuffer(buffer);
            }
        }

        /// <summary>
        /// Called whenever the loaded <see cref="Midi"/> of the <see cref="Model"/> has changed. Refreshes the <see cref="AudioBufferWaveProvider"/> which stores the <see cref="Midi"/>'s audio data.
        /// </summary>
        /// <param name="midi">The midi which have changed.</param>
        private void OnSingleMidiChanged(Midi midi)
        {
            Model.MidiAudioBuffer.Init((int)(Model.Midi.Length * (ProjectModel.StandardSampleRate / 1000f)), ProjectModel.StandardWaveFormat);
            Model.MidiAudioBuffer.ChangeSelection(SingleMidiViewViewModel.Selection.Start, SingleMidiViewViewModel.Selection.End);
            if (Model.MidiAudioBuffer.State == AudioBufferState.Selected || Model.MidiAudioBuffer.State == AudioBufferState.Playing)
            {
                Model.StartMidiAudioRendering();
            }
        }

        #endregion

        #region Saving

        /// <summary>
        /// Saves the model when the parameters have changed.
        /// </summary>
        public void SaveModelOnParametersChanged()
        {
            Model.ConversionSelectionOnly = ConversionSelectionOnly;
            Model.ConversionMinimumDecibelAmplitude.Value = (float)ConversionMinimumDecibelAmplitude.Value;
            Model.ConversionMinimumLength.Value = (int)ConversionMinimumLength.Value;
            Model.ConversionMaximumSilence.Value = (int)ConversionMaximumSilence.Value;
            Model.IsNoteOverwritingEnabled = _isNoteOverwritingEnabled;
            Model.SinusoidParameters.AreFrequenciesFixed = _areFrequenciesFixed;
            Model.SinusoidParameters.MinimumDecibelAmplitude.Value = (float)SinusoidMinimumDecibelAmplitude.Value;
            Model.SinusoidParameters.MinimumLength.Value = (int)SinusoidMinimumLength.Value;
            Model.SinusoidParameters.MaximumSleepingTime.Value = (int)SinusoidMaximumSleepingTime.Value;
            Model.SinusoidParameters.ContinuationRange.Value = (float)SinusoidContinuationRange.Value;
            Model.TransientParameters.Strength.Value = (float)TransientStrength.Value;
            Model.TransientParameters.AdjacencyNumber.Value = (int)TransientAdjacencyNumber.Value;
            Model.TransientParameters.FlagRatio.Value = (float)TransientFlagRatio.Value;
            Model.TransientParameters.IsTransposable = _transientIsTransposable;
            Model.NoiseParameters.SamplingFrequency.Value = (int)NoiseSamplingFrequency.Value;
            ModelSaved?.Invoke(Model);
        }

        #endregion

        #region Init

        /// <summary>
        /// Sets up all the commands this class uses.
        /// </summary>
        private void InitCommands()
        {
            ConvertWaveToMidiCommand = new RelayCommand(() => BeginWaveToMidiConversion());
            ClearMidiCommand = new RelayCommand(() => ClearMidi());
            AddInstrumentCommand = new RelayCommand(() => AddInstrument());
            RemoveInstrumentCommand = new RelayCommand(() => RemoveInstrument());
            UpdateInstrumentCommand = new RelayCommand(() => BeginInstrumentGeneration());
            ClearInstrumentCommand = new RelayCommand(() => ClearInstrument());
            InstrumentGenerationWindow.LoadingCancelled += Computer.CancelInstrumentGenerationTask;
        }

        /// <summary>
        /// Initializes the views.
        /// </summary>
        private void InitViews()
        {
            TimeDomainViewModel.SetUpFollowers(WaveViewViewModel, WaveRulerViewViewModel, SingleMidiViewViewModel);
            SinusoidViewViewModel.SetUpZoomingParameters(800);
            SinusoidViewViewModel.SetUpAltZoomingParameters(1, ProjectModel.TonePerNote, 1, 2);
            SinusoidRulerViewViewModel.SetUpZoomingParameters(800);
            SinusoidRulerViewViewModel.SetUpAltZoomingParameters(1, ProjectModel.TonePerNote, 1, 2);
            TimeDomainViewModel.SetUpFollowers(SinusoidViewViewModel, SinusoidRulerViewViewModel);
            TimeDomainViewModel.SetUpFollowers(TransientViewViewModel, TransientRulerViewViewModel);
            TimeDomainViewModel.SetUpFollowers(NoiseViewViewModel, NoiseRulerViewViewModel);
            TimeDomainViewModel.SetUpFollowers(TransientViewViewModel, NoiseViewViewModel, followSelectionChange: false);
            TimeDomainViewModel.SetUpFollowers(SinusoidViewViewModel, TransientViewViewModel, NoiseViewViewModel, followScroll: false, followZoom: false, followAltZoom: false);
            SingleMidiViewViewModel.CursorType = MidiViewCursorType.Adjustment;
            InstrumentsComboBox.SelectionChanged += LoadInstrument;
            InstrumentViewViewModel.SelectedNoteChanged += BeginNoteGeneration;
        }

        /// <summary>
        /// Initializes the <see cref="AudioBufferBorderViewModel"/>s.
        /// </summary>
        private void InitBufferViewModels()
        {
            WaveAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            SingleMidiAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            SingleMidiAudioBufferViewModel.LeftClicked += (_) => Model.StartMidiAudioRendering();
            InstrumentAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            NoteAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            NoteAudioBufferViewModel.LeftClicked += (_) => Model.StartNoteAudioRendering();
            SinusoidAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            SinusoidAudioBufferViewModel.LeftClicked += (_) => Model.StartSinusoidAudioRendering();
            TransientAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            NoiseAudioBufferViewModel.LeftClicked += SelectAudioBuffer;
            SingleMidiViewViewModel.ModelSaved += OnSingleMidiChanged;
            SingleMidiViewViewModel.PropertyChanged += OnPropertyChanged;
            WaveViewViewModel.PropertyChanged += OnPropertyChanged;
            WaveViewViewModel.MouseMiddleButtonClicked += WaveAudioBufferViewModel.Select;
            SingleMidiViewViewModel.MouseMiddleButtonClicked += SingleMidiAudioBufferViewModel.Select;
            SinusoidViewViewModel.MouseMiddleButtonClicked += SinusoidAudioBufferViewModel.Select;
            TransientViewViewModel.MouseMiddleButtonClicked += TransientAudioBufferViewModel.Select;
            NoiseViewViewModel.MouseMiddleButtonClicked += NoiseAudioBufferViewModel.Select;
        }

        /// <summary>
        /// Initializes the <see cref="AudioBufferWaveProvider"/>s.
        /// </summary>
        private void InitBuffers()
        {
            WaveAudioBufferViewModel.LoadModel(Model.WaveAudioBuffer);
            SingleMidiAudioBufferViewModel.LoadModel(Model.MidiAudioBuffer);
            NoteAudioBufferViewModel.LoadModel(Model.NoteAudioBuffer);
            SinusoidAudioBufferViewModel.LoadModel(Model.SinusoidAudioBuffer);
            TransientAudioBufferViewModel.LoadModel(Model.TransientAudioBuffer);
            NoiseAudioBufferViewModel.LoadModel(Model.NoiseAudioBuffer);
            InstrumentAudioBufferViewModel.LoadModel(Model.InstrumentAudioBuffer);
            WaveViewViewModel.SelectionChanged += Model.WaveAudioBuffer.ChangeSelection;
            SingleMidiViewViewModel.SelectionChanged += Model.MidiAudioBuffer.ChangeSelection;
            SinusoidViewViewModel.SelectionChanged += Model.SinusoidAudioBuffer.ChangeSelection;
            TransientViewViewModel.SelectionChanged += Model.TransientAudioBuffer.ChangeSelection;
            NoiseViewViewModel.SelectionChanged += Model.NoiseAudioBuffer.ChangeSelection;
            SinusoidViewViewModel.SelectionChanged += Model.NoteAudioBuffer.ChangeSelection;
            TransientViewViewModel.SelectionChanged += Model.NoteAudioBuffer.ChangeSelection;
            NoiseViewViewModel.SelectionChanged += Model.NoteAudioBuffer.ChangeSelection;
        }


        /// <summary>
        /// Sets the default slider values.
        /// </summary>
        private void SetDefaultValues()
        {
            _conversionSelectionOnly = Model.ConversionSelectionOnly;
            ConversionMinimumDecibelAmplitude.LoadModel(Model.ConversionMinimumDecibelAmplitude);
            ConversionMinimumLength.LoadModel(Model.ConversionMinimumLength);
            ConversionMaximumSilence.LoadModel(Model.ConversionMaximumSilence);
            _isNoteOverwritingEnabled = Model.IsNoteOverwritingEnabled;
            _areFrequenciesFixed = Model.SinusoidParameters.AreFrequenciesFixed;
            SinusoidMinimumDecibelAmplitude.LoadModel(Model.SinusoidParameters.MinimumDecibelAmplitude);
            SinusoidMaximumSleepingTime.LoadModel(Model.SinusoidParameters.MaximumSleepingTime);
            SinusoidMinimumLength.LoadModel(Model.SinusoidParameters.MinimumLength);
            SinusoidContinuationRange.LoadModel(Model.SinusoidParameters.ContinuationRange);
            TransientStrength.LoadModel(Model.TransientParameters.Strength);
            TransientAdjacencyNumber.LoadModel(Model.TransientParameters.AdjacencyNumber);
            TransientFlagRatio.LoadModel(Model.TransientParameters.FlagRatio);
            _transientIsTransposable = Model.TransientParameters.IsTransposable;
            NoiseSamplingFrequency.LoadModel(Model.NoiseParameters.SamplingFrequency);
            OnPropertyChanged("");
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentBuilderViewModel"/> class.
        /// </summary>
        public InstrumentBuilderViewModel()
        {
            InitCommands();
            InitViews();
            InitBufferViewModels();
        }
        #endregion
    }
}
