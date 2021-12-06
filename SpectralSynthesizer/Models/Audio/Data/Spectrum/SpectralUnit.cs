using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// A unit of a <see cref="Spectrum"/>.
    /// </summary>
    public readonly struct SpectralUnit
    {
        #region Readonly Properties

        /// <summary>
        /// The amplitude value of this spectral unit.
        /// </summary>
        public float Amplitude { get; }

        /// <summary>
        /// The frequency value of this spectral unit.
        /// </summary>
        public float Frequency { get; }

        #endregion

        #region Methods

        #region Rendering

        /// <summary>
        /// Renders this <see cref="SpectralUnit"/>.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="phase">The continously changing phase between 0 and <see cref="SineWaveCache.Length"/>.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, ref double phase)
        {
            float[] buffer = new float[length];
            double waveLength = (double)sampleRate / Frequency;
            double step = Computer.SineWaveCache.Length / waveLength;
            int i = 0;
            double sineIndex = 0;
            double reminder = ((phase % Computer.SineWaveCache.Length) + Computer.SineWaveCache.Length) % Computer.SineWaveCache.Length;
            int wholeIndex = (int)(sineIndex + reminder);
            while (i < length)
            {
                while (i < length && wholeIndex < Computer.SineWaveCache.Length)
                {
                    buffer[i] += Computer.SineWaveCache.Values[wholeIndex] * Amplitude;
                    i++;
                    sineIndex += step;
                    wholeIndex = (int)(sineIndex + reminder);
                }
                reminder = (reminder + sineIndex) - Computer.SineWaveCache.Length;
                sineIndex = 0;
                wholeIndex = (int)(sineIndex + reminder);
            }
            phase = reminder;
            return buffer;
        }

        /// <summary>
        /// Renders a sine wave by linearly interpolating between two <see cref="SpectralUnit"/>s.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="start">The end <see cref="SpectralUnit"/>.</param>
        /// <param name="end">The end <see cref="SpectralUnit"/>.</param>
        /// <param name="length">The length of the rendered sine wave in floats.</param>
        /// <param name="phase">The continously changing phase between 0 and <see cref="SineWaveCache.Length"/>.</param>
        /// <returns>The float array containing the rendered sine wave.</returns>
        public static float[] RenderSineWave(int sampleRate, SpectralUnit start, SpectralUnit end, int length, ref double phase)
        {
            float[] buffer = new float[length];
            float amplitudeStep = (end.Amplitude - start.Amplitude) / length;
            float currentAmplitude = start.Amplitude;
            float startLogarithmicFrequency = Computer.FrequencyToLogarithmicFrequency(start.Frequency);
            float endLogarithmicFrequency = Computer.FrequencyToLogarithmicFrequency(end.Frequency);
            double logarithmicFrequencyStep = (endLogarithmicFrequency - startLogarithmicFrequency) / length;
            double currentLogarithmicFrequency = startLogarithmicFrequency;
            double waveLengthScale = Computer.SineWaveCache.Length / (double)sampleRate;
            int i = 0;
            double sineIndex = 0;
            double reminder = ((phase % Computer.SineWaveCache.Length) + Computer.SineWaveCache.Length) % Computer.SineWaveCache.Length;
            int wholeIndex = (int)(sineIndex + reminder);
            while (i < length)
            {
                while (i < length && wholeIndex < Computer.SineWaveCache.Length)
                {
                    buffer[i] += Computer.SineWaveCache.Values[wholeIndex] * currentAmplitude;
                    i++;
                    sineIndex += Computer.LogarithmicFrequencyToFrequency((float)currentLogarithmicFrequency) * waveLengthScale;
                    currentLogarithmicFrequency += logarithmicFrequencyStep;
                    currentAmplitude += amplitudeStep;
                    wholeIndex = (int)(sineIndex + reminder);
                }
                reminder = (reminder + sineIndex) - Computer.SineWaveCache.Length;
                sineIndex = 0;
                wholeIndex = (int)(sineIndex + reminder);
            }
            phase = reminder;
            return buffer;
        }

        #endregion

        #endregion

        #region Constructor

        [JsonConstructor]
        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralUnit"/> struct.
        /// </summary>
        /// <param name="amplitude">The amplitude of the unit.</param>
        /// <param name="frequency">The frequency of the unit.</param>
        public SpectralUnit(float amplitude, float frequency)
        {
            Amplitude = amplitude;
            Frequency = frequency;
        }

        #endregion
    }

    /// <summary>
    /// <see cref="SpectralUnit.Amplitude"/> comparer for the <see cref="SpectralUnit"/> class.
    /// Sorts in descending form.
    /// </summary>
    public class AmplitudeComparer : IComparer<SpectralUnit>
    {
        public int Compare(SpectralUnit x, SpectralUnit y)
        {
            return y.Amplitude.CompareTo(x.Amplitude);
        }
    }

    /// <summary>
    /// <see cref="SpectralUnit.Frequency"/> comparer for the <see cref="SpectralUnit"/> class.
    /// Sorts in ascending form.
    /// </summary>
    public class FrequencyComparer : IComparer<SpectralUnit>
    {
        public int Compare(SpectralUnit x, SpectralUnit y)
        {
            return x.Frequency.CompareTo(y.Frequency);
        }
    }
}
