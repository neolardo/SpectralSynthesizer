using Newtonsoft.Json;
using SpectralSynthesizer.Models.Audio.Data;
using System.Linq;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// The model of the <see cref="InstrumentBuilderControl"/> and <see cref="InstrumentBuilderDetailControl"/>.
    /// </summary>
    public class InstrumentBuilderModel : BaseModel
    {
        #region Properties

        #region Audio Models

        /// <summary>
        /// The loaded <see cref="Audio.Data.Wave"/>.
        /// </summary>
        public Wave Wave { get; set; }

        /// <summary>
        /// The loaded <see cref="Audio.Data.Midi"/>.
        /// </summary>
        public Midi Midi { get; set; }

        /// <summary>
        /// The loaded <see cref="Audio.Data.Instrument"/>.
        /// </summary>
        public Instrument Instrument { get; set; }

        /// <summary>
        /// The loaded <see cref="Audio.Data.Note"/>.
        /// </summary>
        public Note Note { get; set; }

        #endregion

        #region Audio Buffers

        /// <summary>
        /// The loaded <see cref="Wave"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider WaveAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Midi"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider MidiAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Instrument"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider InstrumentAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Note"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider NoteAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Note"/>'s <see cref="Sinusoid"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider SinusoidAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Note"/>'s <see cref="Transient"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider TransientAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Note"/>'s <see cref="Noise"/> <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider NoiseAudioBuffer { get; } = new AudioBufferWaveProvider();

        #endregion

        #region Wave to Midi Conversion

        /// <summary>
        /// Indicates whether the conversion should only be applied on the wave selected area or not.
        /// </summary>
        public bool ConversionSelectionOnly { get; set; } = true;

        /// <summary>
        /// The minimum decibel amplitude of any possible note to be noticed in the <see cref="Audio.Data.Wave"/> to <see cref="Audio.Data.Midi"/> conversion process.
        /// </summary>
        public Parameter<float> ConversionMinimumDecibelAmplitude { get; } = new Parameter<float>(ProjectModel.GlobalMinimumDecibel * 0.5f, ProjectModel.GlobalMinimumDecibel, 0);

        /// <summary>
        /// The minimum length of any possible note in milliseconds in the <see cref="Audio.Data.Wave"/> to <see cref="Audio.Data.Midi"/> conversion process.
        /// </summary>
        public Parameter<int> ConversionMinimumLength { get; } = new Parameter<int>(400, 50, 5000);

        /// <summary>
        ///The maximum amount of silence in any possible note in milliseconds in the <see cref="Audio.Data.Wave"/> to <see cref="Audio.Data.Midi"/> conversion process.
        /// </summary>
        public Parameter<int> ConversionMaximumSilence { get; } = new Parameter<int>(100, 50, 1000);

        #endregion

        #region Instrument Generation

        /// <summary>
        /// The name of any <see cref="Audio.Data.Instrument"/> by default.
        /// </summary>
        private static string DefaultInstrumentName => "instrument";

        /// <summary>
        /// Indicates whether the <see cref="Note"/>s should be overwritten when updating the selected <see cref="Instrument"/>.
        /// </summary>
        public bool IsNoteOverwritingEnabled { get; set; } = false;

        /// <summary>
        /// The <see cref="Models.SinusoidParameters"/>.
        /// </summary>
        public SinusoidParameters SinusoidParameters { get; } = new SinusoidParameters();

        /// <summary>
        /// The <see cref="Models.TransientParameters"/>.
        /// </summary>
        public TransientParameters TransientParameters { get; } = new TransientParameters();

        /// <summary>
        /// The <see cref="Models.NoiseParameters"/>.
        /// </summary>
        public NoiseParameters NoiseParameters { get; } = new NoiseParameters();

        #endregion

        #endregion

        #region Methods 

        #region Load

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Wave"/> from it's filepath.
        /// </summary>
        /// <param name="filePath">The path to the wave file.</param>
        public void LoadWave(string filePath)
        {
            Wave = new Wave(filePath);
            RenderWaveAudio();
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Midi"/> from it's filepath.
        /// </summary>
        /// <param name="filePath">The path to the midi file.</param>
        public void LoadMidi(string filePath)
        {
            LoadMidi(new Midi(filePath));
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Midi"/>.
        /// </summary>
        /// <param name="midi">The <see cref="Midi"/>.</param>
        public void LoadMidi(Midi midi)
        {
            Midi = midi;
            Midi.MakeToSingle(Wave.Length);
            MidiAudioBuffer.Init((int)(Midi.Length * (ProjectModel.StandardSampleRate / 1000f)), ProjectModel.StandardWaveFormat);
        }

        /// <summary>
        /// Unloads the loaded midi.
        /// </summary>
        public void UnloadMidi()
        {
            Midi = null;
            MidiAudioBuffer.State = DataModels.Enums.AudioBufferState.Empty;
            Computer.CancelWaveToMidiConversionTask();
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Instrument"/>.
        /// </summary>
        /// <param name="instrument">The <see cref="Audio.Data.Instrument"/> to load.</param>
        public void LoadInstrument(Instrument instrument)
        {
            Instrument = instrument;
            LoadNote(null);
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Note"/>.
        /// </summary>
        /// <param name="note">The <see cref="Audio.Data.Note"/> to load.</param>
        public void LoadNote(Note note)
        {
            Note = note;
            if (Note != null)
            {
                NoteAudioBuffer.Init((int)(Note.Length * (ProjectModel.StandardSampleRate / 1000f)), ProjectModel.StandardWaveFormat);
                if (note.Sinusoid != null)
                {
                    SinusoidAudioBuffer.Init((int)(Note.Length * (ProjectModel.StandardSampleRate / 1000f)), ProjectModel.StandardWaveFormat);
                }
                RenderTransientAudio();
                RenderNoiseAudio();
            }
            else
            {
                NoteAudioBuffer.State = DataModels.Enums.AudioBufferState.Empty;
            }
        }

        #endregion

        #region Wave to Midi Conversion

        /// <summary>
        /// Begins the conversion of the  loaded <see cref="Audio.Data.Wave"/> into a <see cref="Audio.Data.Midi"/>.
        /// </summary>
        /// <param name="selection">The start and end ratios of the <see cref="Audio.Data.Wave"/>'s length indicating the relevant part of the conversion.</param>
        public void BeginWaveToMidiConversion((double start, double end) selection)
        {
            Computer.StartWaveToMidiConversionTask(Wave, selection, ConversionMinimumDecibelAmplitude.Value, ConversionMinimumLength.Value, ConversionMaximumSilence.Value);
        }

        /// <summary>
        /// Ends the conversion of the loaded <see cref="Audio.Data.Wave"/> into a <see cref="Audio.Data.Midi"/> and loads the converted <see cref="Audio.Data.Midi"/>.
        /// </summary>
        /// <param name="result">The converted <see cref="Audio.Data.Midi"/>.</param>
        public void EndWaveToMidiConversion(Midi result)
        {
            LoadMidi(result);
        }

        #endregion

        #region Instrument Generation

        /// <summary>
        /// Begins the <see cref="Audio.Data.Instrument"/> generation from the loaded <see cref="Audio.Data.Wave"/> and <see cref="Audio.Data.Midi"/>.
        /// </summary>
        /// <param name="selection">The start and end ratios of the <see cref="Audio.Data.Wave"/>'s length indicating the relevant part of the generation.</param>
        /// <param name="type">The type of models used in the generation.</param>
        public void BeginInstrumentGeneration((double start, double end) selection, InstrumentModelType type)
        {
            Computer.CancelNoteGenerationTask();
            Computer.StartInstrumentGenerationTask(Wave, Midi, selection, type, new SinusoidParameters(SinusoidParameters), new TransientParameters(TransientParameters), new NoiseParameters(NoiseParameters));
        }

        /// <summary>
        /// Clears the selected <see cref="Audio.Data.Instrument"/>.
        /// </summary>
        public void ClearInstrument()
        {
            Instrument?.Clear();
        }

        /// <summary>
        /// Adds a new <see cref="Audio.Data.Instrument"/> to the <see cref="ProjectModel.Instruments"/> list.
        /// </summary>
        public void AddInstrument()
        {
            IoC.Get<ProjectModel>().Instruments.Add(new Instrument(IoC.Get<ProjectModel>().CalculateAvailableInstrumentName(DefaultInstrumentName)));
        }

        /// <summary>
        /// Removes the loaded <see cref="Instrument"/> from the <see cref="ProjectModel.Instruments"/> list and deselects is.
        /// </summary>
        public void RemoveInstrument()
        {
            if (Instrument != null)
            {
                IoC.Get<ProjectModel>().Instruments.Remove(Instrument);
            }
        }

        #endregion

        #region Note Generation

        /// <summary>
        /// Begins the <see cref="Audio.Data.Note"/> generation process.
        /// </summary>
        /// <param name="noteNumber">The note number of the new <see cref="Audio.Data.Note"/>.</param>
        public void BeginNoteGeneration(int noteNumber)
        {
            Computer.StartNoteGenerationTask(Instrument, noteNumber);
        }

        #endregion

        #region Audio Rendering

        /// <summary>
        /// Loads the <see cref="Wave"/>'s data to the <see cref="WaveAudioBuffer"/>.
        /// </summary>
        public void RenderWaveAudio()
        {
            WaveAudioBuffer.Init(Wave.Data.Length, Wave.WaveFormat);
            WaveAudioBuffer.InsertData(Wave.Data, 0);
        }


        /// <summary>
        /// Loads the <see cref="Note.Transient"/>'s data to the <see cref="TransientAudioBuffer"/>.
        /// </summary>
        public void RenderTransientAudio()
        {
            if (Note.Transient != null)
            {
                TransientAudioBuffer.Init(Note.Transient.Wave.Data.Length, Note.Transient.Wave.WaveFormat);
                TransientAudioBuffer.InsertData(Note.Transient.Wave.Data, 0);
            }
        }

        /// <summary>
        /// Loads the <see cref="Note.Noise"/>'s data to the <see cref="NoiseAudioBuffer"/>.
        /// </summary>
        public void RenderNoiseAudio()
        {
            if (Note.Noise != null)
            {
                NoiseAudioBuffer.Init(Note.Noise.Wave.Data.Length, Note.Noise.Wave.WaveFormat);
                NoiseAudioBuffer.InsertData(Note.Noise.Wave.Data, 0);
            }
        }

        /// <summary>
        /// Starts to render audio for the loaded <see cref="Midi"/>.
        /// </summary>
        public void StartMidiAudioRendering()
        {
            if (Midi != null && !MidiAudioBuffer.IsBufferingComplete)
            {
                Computer.StartAudioRenderingTask(Midi, ProjectModel.StandardSampleRate, MidiAudioBuffer);
            }
        }

        /// <summary>
        /// Starts to render audio for the loaded <see cref="Note"/>.
        /// </summary>
        public void StartNoteAudioRendering()
        {
            if (Note != null && !NoteAudioBuffer.IsBufferingComplete)
            {
                Computer.StartAudioRenderingTask(Note, ProjectModel.StandardSampleRate, NoteAudioBuffer);
            }
        }

        /// <summary>
        /// Starts to render audio for the loaded <see cref="Note"/>'s <see cref="Sinusoid"/> model.
        /// </summary>
        public void StartSinusoidAudioRendering()
        {
            if (Note != null && Note.Sinusoid != null && !SinusoidAudioBuffer.IsBufferingComplete)
            {
                Computer.StartAudioRenderingTask(Note.Sinusoid, ProjectModel.StandardSampleRate, SinusoidAudioBuffer);
            }
        }

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentBuilderModel"/> class.
        /// </summary>
        public InstrumentBuilderModel() { }

        [JsonConstructor]
        public InstrumentBuilderModel(AudioBufferWaveProvider waveAudioBuffer, AudioBufferWaveProvider midiAudioBuffer, AudioBufferWaveProvider instrumentAudioBuffer,
            AudioBufferWaveProvider noteAudioBuffer, AudioBufferWaveProvider sinusoidAudioBuffer, AudioBufferWaveProvider transientAudioBuffer, AudioBufferWaveProvider noiseAudioBuffer,
            Parameter<float> conversionMinimumDecibelAmplitude, Parameter<int> conversionMinimumLength, Parameter<int> conversionMaximumSilence,
            SinusoidParameters sinusoidParameters, TransientParameters transientParameters, NoiseParameters noiseParameters)
        {
            MidiAudioBuffer = midiAudioBuffer;
            WaveAudioBuffer = waveAudioBuffer;
            InstrumentAudioBuffer = instrumentAudioBuffer;
            NoteAudioBuffer = noteAudioBuffer;
            SinusoidAudioBuffer = sinusoidAudioBuffer;
            TransientAudioBuffer = transientAudioBuffer;
            NoiseAudioBuffer = noiseAudioBuffer;
            ConversionMinimumDecibelAmplitude = conversionMinimumDecibelAmplitude;
            ConversionMinimumLength = conversionMinimumLength;
            ConversionMaximumSilence = conversionMaximumSilence;
            SinusoidParameters = sinusoidParameters;
            TransientParameters = transientParameters;
            NoiseParameters = noiseParameters;
        }

        #endregion
    }
}
