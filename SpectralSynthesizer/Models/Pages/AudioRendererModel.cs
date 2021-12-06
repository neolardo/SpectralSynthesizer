using Newtonsoft.Json;
using SpectralSynthesizer.Models.Audio.Data;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// The model of the <see cref="AudioRendererControl"/>.
    /// </summary>
    public class AudioRendererModel : BaseModel
    {
        #region Properties

        #region Audio Models

        /// <summary>
        /// The loaded <see cref="Audio.Data.Midi"/>.
        /// </summary>
        public Midi Midi { get; set; }

        /// <summary>
        /// The loaded <see cref="Audio.Data.Instrument"/>.
        /// </summary>
        public Instrument Instrument { get; set; }

        /// <summary>
        /// The loaded <see cref="Audio.Data.Wave"/>.
        /// </summary>
        public Wave Wave { get; set; }

        #endregion

        #region Audio Buffers

        /// <summary>
        /// The loaded <see cref="Midi"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider MidiAudioBuffer { get; } = new AudioBufferWaveProvider();

        /// <summary>
        /// The loaded <see cref="Wave"/>'s <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        public AudioBufferWaveProvider WaveAudioBuffer { get; } = new AudioBufferWaveProvider();

        #endregion

        #region Rendering

        /// <summary>
        /// Indicates whether the audio rendering process should only apply to the selected part of the loaded <see cref="Audio.Data.Midi"/>.
        /// </summary>
        public bool RenderingSelectionOnly { get; set; } = false;

        /// <summary>
        /// Indicates whether the audio rendering process should be rendered as all <see cref="Note"/>s are played until their original length.
        /// </summary>
        public bool RenderingEnableSustain { get; set; } = false;

        /// <summary>
        /// The <see cref="LoadingPercentManager"/> for <see cref="Wave"/> audio rendering process.
        /// </summary>
        public static LoadingPercentManager AudioRendererPercentManager { get; } = new LoadingPercentManager();

        #endregion

        #endregion

        #region Methods 

        #region Load

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Wave"/>.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        public void LoadWave(Wave wave)
        {
            Wave = wave;
            RenderWaveAudio();
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Midi"/> from it's filepath.
        /// </summary>
        /// <param name="filePath">The path to the midi file.</param>
        public void LoadMidi(string filePath)
        {
            Midi = new Midi(filePath);
            MidiAudioBuffer.Init((int)(Midi.Length * (ProjectModel.StandardSampleRate / 1000f)), ProjectModel.StandardWaveFormat);
        }

        /// <summary>
        /// Loads the given <see cref="Audio.Data.Instrument"/>.
        /// </summary>
        /// <param name="instrument">The <see cref="Audio.Data.Instrument"/> to load.</param>
        public void LoadInstrument(Instrument instrument)
        {
            Instrument = instrument;
        }

        #endregion

        #region Audio Rendering

        /// <summary>
        /// Begins the <see cref="Wave"/> audio rendering process.
        /// </summary>
        /// <param name="selection">The start and end ratios of the <see cref="Audio.Data.Midi"/>'s length indicating the relevant part of the renering.</param>
        public void BeginAudioRendering((double start, double end) selection)
        {
            Computer.StartWaveAudioRenderingTask(Midi, Instrument, RenderingSelectionOnly ? selection : (0, 1), RenderingEnableSustain, ProjectModel.StandardWaveFormat);
        }

        /// <summary>
        /// Loads the <see cref="Wave"/>'s data to the <see cref="WaveAudioBuffer"/>.
        /// </summary>
        public void RenderWaveAudio()
        {
            WaveAudioBuffer.Init(Wave.Data.Length, Wave.WaveFormat);
            WaveAudioBuffer.InsertData(Wave.Data, 0);
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

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioRendererModel"/> class.
        /// </summary>
        public AudioRendererModel() { }

        [JsonConstructor]
        public AudioRendererModel(AudioBufferWaveProvider midiAudioBuffer, AudioBufferWaveProvider waveAudioBuffer)
        {
            MidiAudioBuffer = midiAudioBuffer;
            WaveAudioBuffer = waveAudioBuffer;
        }

        #endregion
    }
}
