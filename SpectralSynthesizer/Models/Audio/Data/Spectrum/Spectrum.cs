using MoreLinq.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// A small portion of audio data represented by <see cref="SpectralUnit"/>s.
    /// </summary>
    public class Spectrum : BaseModel
    {
        #region Properties

        /// <summary>
        /// All the spectral units this spectrum consists of.
        /// </summary>
        public List<SpectralUnit> Spectrals { get; set; } = new List<SpectralUnit>();

        /// <summary>
        /// The fundamental spectral containing the fundamental frequency of the spectrum.
        /// </summary>
        public SpectralUnit FundamentalSpectral { get; set; }

        #endregion

        #region Methods

        #region Creation

        #region Spectral Peak Calculation

        /// <summary>
        /// Calculates the <see cref="SpectralUnit"/>s of a given Fourier transformed <see cref="Complex"/> vector. This function can be cancelled.
        /// </summary>
        /// <param name="input">The Fourier transformed <see cref="Complex"/> input vector.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="waveSampleRate">The samplerate of the Fourier transformed <see cref="Wave"/>.</param>
        /// <param name="nonZeroLength">The length of the non-zero part of the data.</param>
        /// <param name="minLogFreqDist">The minimum logarithmic frequency distance between two peaks.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        /// <returns>The <see cref="SpectralUnit"/> peaks of the given Fourier transformed <see cref="Complex"/> vector.</returns>
        private List<SpectralUnit> CalculateSpectralPeaks(Complex[] input, float minimumDecibel, int waveSampleRate, int nonZeroLength, float minLogFreqDist, WindowingType windowType, CancellationToken token)
        {
            double ampScale = 1.0 / ((nonZeroLength / 2.0) * Computer.HannWindowCache.GetAreaRatioOf(windowType));
            float freqScale = (float)waveSampleRate / input.Length;
            int startIndex = 0;
            int endIndex = (int)Computer.ClampMax(ProjectModel.HighestFrequency / freqScale, input.Length / 2);
            var peaks = new List<SpectralUnit>();
            for (int i = startIndex; i < endIndex - 2; i++)
            {
                try
                {
                    if (input[i + 1].Magnitude >= input[i].Magnitude && input[i + 1].Magnitude >= input[i + 2].Magnitude)
                    {
                        double p = Computer.GetParabolicParameterFromThreeMagnitudes(Computer.PCMAmplitudeToDecibel((float)(input[i].Magnitude * ampScale)), Computer.PCMAmplitudeToDecibel((float)(input[i + 1].Magnitude * ampScale)), Computer.PCMAmplitudeToDecibel((float)(input[i + 2].Magnitude * ampScale)));
                        if (!Double.IsNaN(p))
                        {
                            float magnitude = (float)(input[i + 1].Magnitude - 0.25 * (input[i].Magnitude - input[i + 2].Magnitude) * p);
                            float decibelAmp = Computer.PCMAmplitudeToDecibel((float)(magnitude * ampScale));
                            if (decibelAmp > minimumDecibel)
                            {
                                var spectral = new SpectralUnit(Computer.DecibelToPCMAmplitude(decibelAmp), (float)((i + 1 + p) * freqScale));
                                if (peaks.Count > 0 && Math.Abs(Computer.FrequencyToLogarithmicFrequency(peaks[peaks.Count - 1].Frequency) - Computer.FrequencyToLogarithmicFrequency(spectral.Frequency)) <= minLogFreqDist)
                                {
                                    if (spectral.Amplitude > peaks[peaks.Count - 1].Amplitude)
                                    {
                                        peaks[peaks.Count - 1] = spectral;
                                    }
                                }
                                else
                                {
                                    peaks.Add(spectral);
                                }
                            }
                        }
                    }
                }
                catch (ArithmeticException)
                { }
                token.ThrowIfCancellationRequested();
            }
            return peaks;
        }


        /// <summary>
        /// Calculates the <see cref="SpectralUnit"/>s of a given Fourier transformed <see cref="Complex"/> vector at the given frequencies. This function can be cancelled.
        /// </summary>
        /// <param name="input">The Fourier transformed <see cref="Complex"/> input vector.</param>
        /// <param name="frequencies">The array of frequencies where the peaks are detected at.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="waveSampleRate">The samplerate of the Fourier transformed <see cref="Wave"/>.</param>
        /// <param name="nonZeroLength">The length of the non-zero part of the data.</param>
        /// <param name="minLogFreqDist">The minimum logarithmic frequency distance between two peaks.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        /// <returns>The <see cref="SpectralUnit"/> peaks of the given Fourier transformed <see cref="Complex"/> vector.</returns>
        private List<SpectralUnit> CalculateSpectralPeaks(Complex[] input, float[] frequencies, float minimumDecibel, int waveSampleRate, int nonZeroLength, float minLogFreqDist, WindowingType windowType, CancellationToken token)
        {
            double ampScale = 1.0 / ((nonZeroLength / 2.0) * Computer.HannWindowCache.GetAreaRatioOf(windowType));
            float freqScale = (float)waveSampleRate / input.Length;
            var peaks = new List<SpectralUnit>();
            foreach (var freq in frequencies)
            {
                int startIndex = (int)Computer.ClampMax(Computer.LogarithmicFrequencyToFrequency(Computer.FrequencyToLogarithmicFrequency(freq) - minLogFreqDist) / freqScale, input.Length / 2);
                int endIndex = (int)Computer.ClampMax(Computer.LogarithmicFrequencyToFrequency(Computer.FrequencyToLogarithmicFrequency(freq) + minLogFreqDist) / freqScale, input.Length / 2);
                float amp = (float)(input.Skip(startIndex).Take(endIndex - startIndex + 1).Select(_ => _.Magnitude).Max() * ampScale);
                for (int i = startIndex; i <= endIndex - 2; i++)
                {
                    try
                    {
                        if (input[i + 1].Magnitude >= input[i].Magnitude && input[i + 1].Magnitude >= input[i + 2].Magnitude)
                        {
                            double p = Computer.GetParabolicParameterFromThreeMagnitudes(Computer.PCMAmplitudeToDecibel((float)(input[i].Magnitude * ampScale)), Computer.PCMAmplitudeToDecibel((float)(input[i + 1].Magnitude * ampScale)), Computer.PCMAmplitudeToDecibel((float)(input[i + 2].Magnitude * ampScale)));
                            if (!Double.IsNaN(p))
                            {
                                amp = Math.Max(amp, (float)((input[i + 1].Magnitude - 0.25 * (input[i].Magnitude - input[i + 2].Magnitude) * p) * ampScale));
                            }
                        }
                    }
                    catch (ArithmeticException)
                    { }
                    token.ThrowIfCancellationRequested();
                }
                if (Computer.PCMAmplitudeToDecibel(amp) > minimumDecibel)
                {
                    peaks.Add(new SpectralUnit(amp, freq));
                }
            }
            return peaks;
        }


        #endregion

        /// <summary>
        /// Calculates the fundamental <see cref="SpectralUnit"/> of this <see cref="Spectrum"/>.
        /// </summary>
        /// <returns>The fundamental <see cref="SpectralUnit"/>.</returns>
        private SpectralUnit CalculateFundamentalSpectral()
        {
            return Spectrals.Count > 0 ? Spectrals.MaxBy(_ => _.Amplitude).ToArray().First() : new SpectralUnit();
        }

        #endregion

        /// <summary>
        /// Converts this spectrum's spectrals to amplitude float array.
        /// </summary>
        /// <param name="tonePerNote">The number of needed subnotes in each note.</param>
        /// <returns>The float amplitude array.</returns>
        public float[] ToAmplitudeArray(int tonePerNote)
        {
            float[] amplitudes = new float[ProjectModel.TotalNoteNumber];
            foreach (var spec in Spectrals)
            {
                int ind = Computer.FrequencyToDiscreteFrequency(spec.Frequency) / tonePerNote;
                if (ind < amplitudes.Length)
                {
                    amplitudes[ind] = Math.Max(spec.Amplitude, amplitudes[ind]);
                }
            }
            return amplitudes;
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            Spectrum spectrum = new Spectrum();
            {
                FundamentalSpectral = new SpectralUnit(this.FundamentalSpectral.Amplitude, this.FundamentalSpectral.Frequency);
            }
            foreach (SpectralUnit spectral in this.Spectrals)
                spectrum.Spectrals.Add(new SpectralUnit(spectral.Amplitude, spectral.Frequency));
            return spectrum;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new blank instance of the <see cref="Spectrum"/> class.
        /// </summary>
        [JsonConstructor]
        public Spectrum() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class from the relevant part of a <see cref="float[]"/> wave data. This function can be cancelled.
        /// </summary>
        /// <param name="waveData">The <see cref="float[]"/> wave data input centered at the time origin.</param>
        /// <param name="samplerate">The samplerate of the wavedata input.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        public Spectrum(float[] waveData, int samplerate, float minimumDecibel, WindowingType windowType, CancellationToken token)
        {
            var filteredArray = Computer.HannWindowCache.Apply(waveData, windowType);
            var transformedArray = Computer.FourierTransform(filteredArray.ToComplexArray().ZeroPad(), token);
            float minLogFreqDist = 0.125f;
            minimumDecibel = Math.Max(minimumDecibel, Computer.PCMAmplitudeToDecibel(waveData.Select(_ => MathF.Abs(_)).Max()) + ProjectModel.RelativeMinimumDecibel);
            Spectrals = CalculateSpectralPeaks(transformedArray, minimumDecibel, samplerate, filteredArray.Length, minLogFreqDist, windowType, token);
            FundamentalSpectral = CalculateFundamentalSpectral();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class from the relevant part of a <see cref="float[]"/> wave data. This function can be cancelled.
        /// </summary>
        /// <param name="waveData">The <see cref="float[]"/> wave data input centered at the time origin.</param>
        /// <param name="frequencies">The array of frequencies where the peaks are detected at.</param>
        /// <param name="samplerate">The samplerate of the wavedata input.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        public Spectrum(float[] waveData, float[] frequencies, int samplerate, float minimumDecibel, WindowingType windowType, CancellationToken token)
        {
            var filteredArray = Computer.HannWindowCache.Apply(waveData, windowType);
            var transformedArray = Computer.FourierTransform(filteredArray.ToComplexArray().ZeroPad(), token);
            float minLogFreqDist = 0.125f;
            minimumDecibel = Math.Max(minimumDecibel, Computer.PCMAmplitudeToDecibel(waveData.Select(_ => MathF.Abs(_)).Max()) + ProjectModel.RelativeMinimumDecibel);
            Spectrals = CalculateSpectralPeaks(transformedArray, frequencies, minimumDecibel, samplerate, filteredArray.Length, minLogFreqDist, windowType, token);
            FundamentalSpectral = CalculateFundamentalSpectral();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class from the relevant part of a <see cref="float[]"/> wave data. The <see cref="SpectralUnit.Amplitude"/>s are not restricted. This function can be cancelled.
        /// </summary>
        /// <param name="waveData">The <see cref="float[]"/> wave data input centered at the time origin.</param>
        /// <param name="samplerate">The samplerate of the wavedata input.</param>
        /// <param name="windowType">The type of the windowing to apply to the given <see cref="Wave"/>.</param>
        /// <param name="token">The token to cancel this function.</param>
        public Spectrum(float[] waveData, int samplerate, WindowingType windowType, CancellationToken token)
        {
            var filteredArray = Computer.HannWindowCache.Apply(waveData, windowType);
            var transformedArray = Computer.FourierTransform(filteredArray.ToComplexArray().ZeroPad(), token);
            float minLogFreqDist = 0.125f;
            Spectrals = CalculateSpectralPeaks(transformedArray, ProjectModel.GlobalMinimumDecibel, samplerate, filteredArray.Length, minLogFreqDist, windowType, token);
            FundamentalSpectral = CalculateFundamentalSpectral();
        }

        #endregion
    }
}