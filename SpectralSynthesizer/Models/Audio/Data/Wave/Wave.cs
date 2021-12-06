using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents a wave audio data.
    /// </summary>
    public class Wave : TimeDomainModel
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The location of the wave file.
        /// </summary>
        public string FilePath { get; private set; } = "";

        [JsonIgnore]
        /// <summary>
        /// The name of the wave file.
        /// </summary>
        public string FileName
        {
            get
            {
                if (FilePath.Contains('\\'))
                    return FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
                else if (FilePath.Contains('/'))
                    return FilePath.Substring(FilePath.LastIndexOf('/') + 1);
                else
                    return FilePath;
            }
        }

        [JsonConverter(typeof(ArrayReferencePreservngConverter))]
        /// <summary>
        /// The float data array of the wave.
        /// </summary>
        public float[] Data { get; set; }

        [JsonConverter(typeof(WaveFormatConverter))]
        /// <summary>
        /// The waveformat of the wave.
        /// </summary>
        public WaveFormat WaveFormat { get; set; }

        #endregion

        #region Methods

        #region Creation

        /// <summary>
        /// Loads the wave from it's <see cref="FilePath"/> property.
        /// </summary>
        private void LoadFromFilePath()
        {
            var nwave = new AudioFileReader(FilePath);
            Length = (int)nwave.TotalTime.TotalMilliseconds;
            WaveFormat = nwave.WaveFormat;
            // Load the data
            int bytecount = (int)nwave.Length;
            byte[] byteArray = new byte[bytecount];
            nwave.Read(byteArray, 0, bytecount);
            int bytesForSamp = nwave.WaveFormat.BitsPerSample / 8;
            int samps = bytecount / bytesForSamp;
            float[] asFloat = null;
            switch (nwave.WaveFormat.BitsPerSample) // bitdepth
            {
                case 64:
                    double[]
                    asDouble = new double[samps];//bytes/8
                    Buffer.BlockCopy(byteArray, 0, asDouble, 0, bytecount);
                    asFloat = Array.ConvertAll(asDouble, e => (float)e);
                    break;
                case 32:
                    asFloat = new float[samps];//bytes/4
                    Buffer.BlockCopy(byteArray, 0, asFloat, 0, bytecount);
                    break;
                case 16:
                    Int16[]
                    asInt16 = new Int16[samps];//bytes/2
                    Buffer.BlockCopy(byteArray, 0, asInt16, 0, bytecount);
                    asFloat = Array.ConvertAll(asInt16, e => e / 32760.0f);
                    break;
            }
            Data = asFloat;
            nwave.Dispose();
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{int,float[]}"/> containing the rendered <see cref="Note"/>'s as values and the corresponding note numbers as keys.
        /// The <see cref="Note"/>s are rendered using the given <see cref="Instrument"/>.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/>.</param>
        /// <param name="selectedNotes">The list of <see cref="MidiNote"/> list.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The <see cref="Dictionary{int,float[]}"/>.</returns>
        private static Dictionary<int, float[]> GetReneredNoteDictionary(Instrument instrument, List<MidiNote> selectedNotes, int sampleRate, CancellationToken token)
        {
            double percentRatio = 1.0 / selectedNotes.Select(_ => _.NoteNumber).Distinct().Count();
            AudioRendererModel.AudioRendererPercentManager.PercentStepRatio *= percentRatio;
            var renderedNoteDictionary = selectedNotes.Select(_ => _.NoteNumber).Distinct().ToDictionary(noteNumber => noteNumber,
                noteNumber =>
                {
                    var n = instrument.GetNoteAt(noteNumber, token);
                    var renderedNote = n.Render(sampleRate, (int)(n.Length / 1000.0 * sampleRate), token);
                    AudioRendererModel.AudioRendererPercentManager.LoadStep();
                    return renderedNote;
                });
            AudioRendererModel.AudioRendererPercentManager.PercentStepRatio /= percentRatio;
            return renderedNoteDictionary;
        }

        #endregion

        #region Selection

        /// <summary>
        /// Gets the selected part of the <see cref="Data"/>.
        /// </summary>
        /// <param name="selection">The selection start and end ratio.</param>
        /// <returns>The selected part of the <see cref="Data"/>.</returns>
        public float[] GetSelectedData((double Start, double End) selection)
        {
            int length = (int)((selection.End - selection.Start) * Data.Length);
            float[] selectedData = new float[length];
            Array.Copy(Data, (int)(Data.Length * selection.Start), selectedData, 0, length);
            return selectedData;
        }

        /// <summary>
        /// Gets the selected part of the <see cref="Data"/>.
        /// </summary>
        /// <param name="start">The start of the selection in milliseconds.</param>
        /// <param name="end">The end of the selection in milliseconds.</param>
        /// <returns>The selected part of the <see cref="Data"/>.</returns>
        public float[] GetSelectedData(int start, int end)
        {
            (double Start, double End) selection = (Math.Clamp(start / (double)Length, 0, 1), Math.Clamp(end / (double)Length, 0, 1));
            return GetSelectedData(selection);
        }

        #endregion

        #region Amplitude

        /// <summary>
        /// Gets the maximum amplitude of this <see cref="Wave"/> at the relevant part of it's wave data.
        /// </summary>
        /// <param name="startIndex">The start index of the relevant part of the wave data.</param>
        /// <param name="endIndex">The end index of the relevant part of the wave data.</param>
        /// <returns>The maximum amplitude of the given <see cref="Wave"/> at the relevant part of the wave data.</returns>
        public float GetMaximumAmplitude(int startIndex, int endIndex)
        {
            float max = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                if (Math.Abs(this.Data[i]) > max)
                    max = Math.Abs(this.Data[i]);
            }
            return max;
        }

        /// <summary>
        /// Gets the maximum amplitude of this <see cref="Wave"/>.
        /// </summary>
        /// <returns>The maximum amplitude of the given <see cref="Wave"/>.</returns>
        public float GetMaximumAmplitude() => GetMaximumAmplitude(0, Data.Length - 1);

        #endregion

        #region Resampling

        /// <summary>
        /// Resamples this wave.
        /// </summary>
        /// <param name="newWaveFormat">The target <see cref="WaveFormat"/>.</param>
        public void Resample(WaveFormat newWaveFormat)
        {
            double scale = (WaveFormat.SampleRate * WaveFormat.Channels) / (double)(newWaveFormat.SampleRate * newWaveFormat.Channels);
            int newLength = (int)(Data.Length / scale);
            double step = scale;
            double doubleIndex = 0;
            float[] newData = new float[newLength];
            for (int i = 0; i < newData.Length && Math.Ceiling(doubleIndex) < Data.Length; i++)
            {
                newData[i] = Computer.Lerp(Data[(int)Math.Floor(doubleIndex)], Data[(int)Math.Ceiling(doubleIndex)], (doubleIndex - Math.Floor(doubleIndex)) / step);
                doubleIndex += step;
            }
            WaveFormat = newWaveFormat;
            Data = newData;
        }

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new Wave(this);
        }


        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Wave"/> class by deep copying the given <see cref="Wave"/>.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/> to deep copy.</param>
        public Wave(Wave wave)
        {
            WaveFormat = new WaveFormat(wave.WaveFormat.SampleRate, wave.WaveFormat.BitsPerSample, wave.WaveFormat.Channels);
            FilePath = wave.FilePath;
            Length = wave.Length;
            Data = (float[])wave.Data.Clone();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wave"/> class from the given wave file.
        /// </summary>
        /// <param name="filePath">The full path to the wave file.</param>
        public Wave(string filePath)
        {
            FilePath = filePath;
            LoadFromFilePath();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wave"/> class from the given wave data.
        /// </summary>
        /// <param name="data">The <see cref="float[]"/> data of the wave.</param>
        /// <param name="waveFormat">The <see cref="WaveFormat"/> of the wave.</param>
        public Wave(float[] data, WaveFormat waveFormat)
        {
            Data = data;
            WaveFormat = waveFormat;
            Length = (int)(((double)data.Length / (WaveFormat.SampleRate * WaveFormat.Channels)) * 1000.0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wave"/> class containing the given <see cref="Midi"/> melody played by the given <see cref="Instrument"/>. This function can be cancelled.
        /// </summary>
        /// <param name="midi">The <see cref="Midi"/>.</param>
        /// <param name="instrument">The <see cref="Instrument"/>.</param>
        /// <param name="selection">The selection start and end ratio.</param>
        /// <param name="enableSustain">Indicates whether sustain mode is enabled.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        public Wave(Midi midi, Instrument instrument, (double Start, double End) selection, bool enableSustain, WaveFormat waveFormat, CancellationToken token)
        {
            WaveFormat = waveFormat;
            int sampleRate = waveFormat.SampleRate * waveFormat.Channels;
            Length = (int)((selection.End - selection.Start) * midi.Length);
            Data = new float[(int)((Length / 1000f) * sampleRate)];
            int startOffset = (int)(((selection.Start * midi.Length) / 1000f) * sampleRate);
            var selectedNotes = midi.GetSelectedNotes(selection);
            AudioRendererModel.AudioRendererPercentManager.Init(0.8);
            var renderedNoteDictionary = GetReneredNoteDictionary(instrument, selectedNotes, sampleRate, token);
            double percentRatio = 1.0 / selectedNotes.Count * 0.2;
            AudioRendererModel.AudioRendererPercentManager.PercentStepRatio *= percentRatio;
            foreach (var midiNote in selectedNotes)
            {
                int noteStart = Math.Max((int)(midiNote.Start * (sampleRate / 1000f)), startOffset);
                int midiNoteLength = (int)(midiNote.Length * (sampleRate / 1000f));
                var renderedNote = renderedNoteDictionary[midiNote.NoteNumber].Normalize(Computer.DecibelToPCMAmplitude(Computer.PCMAmplitudeToDecibel(midiNote.Amplitude) - ProjectModel.MaximumSingleDecibelAmplitudeDelta)).FadeOut();
                int noteLength = Math.Min(noteStart + renderedNote.Length, startOffset + Data.Length) - noteStart;
                if (!enableSustain)
                {
                    noteLength = Math.Min(noteStart + midiNoteLength, noteLength);
                }
                for (int i = 0; i < noteLength; i++)
                {
                    Data[noteStart + i - startOffset] += renderedNote[i];
                }
                AudioRendererModel.AudioRendererPercentManager.LoadStep();
                token.ThrowIfCancellationRequested();
            }
        }


        /// <summary>
        /// Initializes a new blank instance of the <see cref="Wave"/> class.
        /// </summary>
        [JsonConstructor]
        public Wave() { }

        #endregion
    }
}
