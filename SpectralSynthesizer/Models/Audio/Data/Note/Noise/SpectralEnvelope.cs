using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents the amplitude envelope of a <see cref="Spectrum"/>.
    /// </summary>
    public class SpectralEnvelope : BaseModel
    {
        #region Properties

        #region Static

        /// <summary>
        /// The number of notes a single amplitude value represents.
        /// </summary>
        public static int NotePerPoint => 1;

        /// <summary>
        /// The length of the <see cref="Amplitudes"/> array.
        /// </summary>
        public static int Resolution => ProjectModel.TotalNoteNumber / NotePerPoint;


        #endregion

        [JsonConverter(typeof(ArrayReferencePreservngConverter))]
        /// <summary>
        /// The amplitude array.
        /// </summary>
        public float[] Amplitudes { get; set; } = new float[Resolution];

        #endregion

        #region Methods

        #region Creation

        /// <summary>
        /// Calculates the <see cref="SpectralUnit"/> medians of a given Fourier transformed <see cref="Complex"/> vector.
        /// </summary>
        /// <param name="input">The Fourier transformed <see cref="Complex"/> input vector.</param>
        /// <param name="waveSampleRate">The samplerate of the Fourier transformed <see cref="Wave"/>.</param>
        /// <param name="nonZeroLength">The length of the non-zero part of the data.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        /// <returns>The <see cref="SpectralUnit"/> peaks of the given Fourier transformed <see cref="Complex"/> vector.</returns>
        private List<SpectralUnit> CalculateSpectralMedians(Complex[] input, int waveSampleRate, int nonZeroLength, WindowingType windowType, CancellationToken token)
        {
            float ampScale = 1f / ((nonZeroLength / 2f) * Computer.HannWindowCache.GetAreaRatioOf(windowType));
            float freqScale = (float)waveSampleRate / input.Length;
            int startIndex = (int)(Computer.NoteToFrequency(0) / freqScale);
            int endIndex = (int)Computer.ClampMax(ProjectModel.HighestFrequency / freqScale, input.Length / 2);
            var medians = new List<SpectralUnit>();
            int i = startIndex;
            int currentNoteNumber = 1;
            var tempList = new List<float>();
            int nextStartIndex = startIndex;
            while (i < endIndex && currentNoteNumber < ProjectModel.TotalNoteNumber)
            {
                tempList.Clear();
                i = nextStartIndex;
                int nextEndIndex = (int)Computer.ClampMax(Computer.NoteToFrequency(currentNoteNumber) / freqScale, endIndex);
                while (i <= nextEndIndex)
                {
                    tempList.AddInOrder((float)input[i].Magnitude * ampScale);
                    i++;
                }
                nextStartIndex = nextEndIndex;
                if (tempList.Count > 0)
                {
                    float medianAmp = tempList[tempList.Count / 2];
                    medians.Add(new SpectralUnit(medianAmp, Computer.NoteToFrequency(currentNoteNumber - 1)));
                }
                currentNoteNumber += NotePerPoint;
                token.ThrowIfCancellationRequested();
            }
            return medians;
        }

        /// <summary>
        /// Generates the amplitude values of this <see cref="SpectralEnvelope"/> from the given <see cref="SpectralUnit"/> list.
        /// </summary>
        /// <param name="spec">The <see cref="SpectralUnit"/> list.</param>
        private void GenerateFromSpectrals(List<SpectralUnit> spectrals)
        {
            if (spectrals.Count > 0)
            {
                if (spectrals.Count == 1)
                {
                    Amplitudes[Computer.FrequencyToNote(spectrals[0].Frequency) / NotePerPoint] = spectrals[0].Amplitude;
                }
                else
                {
                    spectrals.Sort(new FrequencyComparer());
                    for (int specInd = 0; specInd < spectrals.Count - 1; specInd++)
                    {
                        int startIndex = Computer.FrequencyToNote(spectrals[specInd].Frequency) / NotePerPoint;
                        int endIndex = Computer.FrequencyToNote(spectrals[specInd + 1].Frequency) / NotePerPoint;
                        double range = endIndex - startIndex;
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            Amplitudes[i] = Computer.Lerp(spectrals[specInd].Amplitude, spectrals[specInd + 1].Amplitude, (i - startIndex) / range);
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Converts this <see cref="SpectralEnvelope"/> to a <see cref="Complex"/> array to make it ready to be used for inverse Fourier transformation.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array.</param>
        /// <param name="sampleRate">The desired sample rate of the inverse Fourier transformed result.</param>
        /// <returns>The <see cref="Complex"/> array.</returns>
        public Complex[] ToComplexArray(int minimumLength, int sampleRate)
        {
            Complex[] output = (new Complex[minimumLength]).ZeroPad();
            float ampScale = 1f / (output.Length / 2f);
            float freqScale = (float)sampleRate / output.Length;
            int minimumFreqInd = Math.Max((int)freqScale, 1);
            for (int ampInd = 0; ampInd < Amplitudes.Length - 1; ampInd++)
            {
                int startFreqInd = (int)(Computer.NoteToFrequency(ampInd * NotePerPoint) / freqScale);
                int endFreqInd = Math.Min((int)(Computer.NoteToFrequency((ampInd + 1) * NotePerPoint) / freqScale), output.Length / 2);
                if (startFreqInd > minimumFreqInd)
                {
                    for (int i = startFreqInd; i < endFreqInd; i++)
                    {
                        double phase = (float)(Computer.R.NextDouble() * 2.0 * Math.PI);
                        double amp = Computer.Lerp(Amplitudes[ampInd], Amplitudes[ampInd + 1], (i - startFreqInd) / (double)endFreqInd);
                        output[i] = Complex.FromPolarCoordinates(amp / ampScale, phase);
                        output[output.Length - 1 - (i - 1)] = Complex.Conjugate(output[i]);
                    }
                }
            }
            return output;
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new SpectralEnvelope((float[])Amplitudes.Clone());
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new blank instance of the <see cref="SpectralEnvelope"/> class.
        /// </summary>
        [JsonConstructor]
        public SpectralEnvelope() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralEnvelope"/> class from the given amplitude array.
        /// </summary>
        /// <param name="amplitudes">The envelope's amplitude array.</param>
        public SpectralEnvelope(float[] amplitudes)
        {
            Amplitudes = amplitudes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralEnvelope"/> class from the given <see cref="float[]"/> wave sample. This function can be cancelled.
        /// </summary>
        /// <param name="sample">The <see cref="float[]"/> wave sample.</param>
        /// <param name="sampleRate">The sample rate of the wave data.</param>
        /// <param name="token">The <see cref="CancellationToken"/>.</param>
        public SpectralEnvelope(float[] sample, int sampleRate, CancellationToken token)
        {
            var windowType = WindowingType.Full;
            var filteredArray = Computer.HannWindowCache.Apply(sample, windowType);
            var transformedArray = Computer.FourierTransform(filteredArray.ToComplexArray().ZeroPad(), token);
            GenerateFromSpectrals(CalculateSpectralMedians(transformedArray, sampleRate, filteredArray.Length, windowType, token));
        }

        #endregion
    }
}
