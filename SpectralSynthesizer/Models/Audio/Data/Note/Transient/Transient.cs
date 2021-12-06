using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents the transient model of any <see cref="Note"/>.
    /// </summary>
    public class Transient : BaseModel
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The <see cref="Transient"/> model in its <see cref="Data.Wave"/> form.
        /// </summary>
        public Wave Wave { get; private set; }

        [JsonProperty]
        /// <summary>
        /// Inidicates whether this <see cref="Transient"/> is transposable or not.
        /// </summary>
        public bool IsTransposable { get; }

        #endregion

        #region Methods

        #region Creation

        #region Transient Separating Fuctions

        /// <summary>
        /// The T minus function.
        /// </summary>
        /// <param name="array">The complex fft bin array.</param>
        /// <returns>The float T minus function values.</returns>
        private static float[] TMinus(Complex[] array)
        {
            float[] result = new float[array.Length];
            for (int i = 1; i < array.Length; i++)
            {
                result[i] = (float)(array[i].Magnitude - array[i - 1].Magnitude);
            }
            return result;
        }

        /// <summary>
        /// The T plus function.
        /// </summary>
        /// <param name="array">The complex fft bin array.</param>
        /// <returns>The float T plus function values.</returns>
        private static float[] TPlus(Complex[] array)
        {
            float[] result = new float[array.Length];
            for (int i = 0; i < array.Length - 1; i++)
            {
                result[i] = (float)(array[i].Magnitude - array[i + 1].Magnitude);
            }
            return result;
        }

        /// <summary>
        /// The F function.
        /// </summary>
        /// <param name="array">The complex fft bin array.</param>
        /// <param name="adjacencyNumber">The number of neightbours to use in the calculation.</param>
        /// <returns>The F function's values.</returns>
        private static float[] F(Complex[] array, int adjacencyNumber)
        {
            float[] result = new float[array.Length];
            var tMinus = TMinus(array);
            var tPlus = TPlus(array);
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = i - adjacencyNumber; j <= i + adjacencyNumber; j++)
                {
                    if (j >= 0 && j < array.Length)
                    {
                        result[i] += (1 + Math.Sign(tMinus[j])) * tMinus[j] + (1 + Math.Sign(tPlus[j])) * tPlus[j];
                    }
                }
                result[i] *= 0.5f;
            }
            return result;
        }
        /// <summary>
        /// The threshold function.
        /// </summary>
        /// <param name="fValues">The array containing the precalculated F values.</param>
        /// <param name="adjacencyNumber">The number of neightbours to use in the calculation.</param>
        /// <param name="strength">The minimum strength of a transient to detect.</param>
        /// <returns>The threshold function's values.</returns>
        private static float[] Threshold(float[] fValues, int adjacencyNumber, float strength)
        {
            float[] result = new float[fValues.Length];
            for (int i = 0; i < fValues.Length; i++)
            {
                int count = 0;
                for (int j = i - adjacencyNumber; j <= i + adjacencyNumber; j++)
                {
                    if (j >= 0 && j < fValues.Length)
                    {
                        result[i] += fValues[j];
                        count++;
                    }
                }
                result[i] = strength * (result[i] / count);
            }
            return result;
        }

        /// <summary>
        /// Flags the given element of the complex array if it's <see cref="F"/> value is larger than the <see cref="Threshold"/>.
        /// </summary>
        /// <param name="f">The f value.</param>
        /// <param name="threshold">The threshold value.</param>
        /// <returns><see langword="true"/>if the given element of the complex array if it's <see cref="F"/> value is larger than the <see cref="Threshold"/>, otherwise <see langword="false"/>.</returns>
        private static bool Flag(float f, float threshold)
        {
            return f > threshold;
        }

        /// <summary>
        /// Determines that the given sample is a transient sample or not.
        /// </summary>
        /// <param name="flagCount">The number of flags in the current sample.</param>
        /// <param name="binCount">The number of fft bins in the current sample.</param>
        /// <param name="flagRatio">The flag ratio.</param>
        /// <returns><see langword="true"/> if the given sample is a transient sample, otherwise <see langword="false"/>.</returns>
        private static bool IsTransientSample(int flagCount, int binCount, float flagRatio)
        {
            return flagCount / (float)binCount > flagRatio;
        }
        /// <summary>
        /// Pinches off a small amount of this <see cref="Complex"/> array. Return the pinched off values and stores the residual back into the original array.
        /// </summary>
        /// <param name="original">The <see cref="Complex"/> array.</param>
        /// <param name="pinchRatio">The ratio amount of the pinch.</param>
        /// <returns>The pinched off <see cref="Complex"/> array.</returns>
        private static Complex[] PinchOff(Complex[] original, float pinchRatio)
        {
            var residual = new Complex[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                residual[i] = original[i] * pinchRatio;
                original[i] = original[i] * (1.0 - pinchRatio);
            }
            return residual;
        }

        /// <summary>
        /// Separates the transient signal from the original <see cref="Data.Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="originalComplexArrays">The original <see cref="Data.Wave"/> in it's stft form.</param>
        /// <param name="strength">The strength of the detectable transient.</param>
        /// <param name="adjacencyNumber">The number of adjacent fft bins used in the calculation.</param>
        /// <param name="flagRatio">The minimum ratio of fft bins needed to flag a sample as a transient sample.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns>The transient <see cref="Data.Wave"/> in it's stft form.</returns>
        private static List<Complex[]> SeparateTransientFromWave(List<Complex[]> originalComplexArrays, float strength, int adjacencyNumber, float flagRatio, CancellationToken token)
        {
            int binCount = originalComplexArrays[0].Length;
            double loadingPercent = 1.0 / binCount;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var transientComplexArrays = new List<Complex[]>();
            foreach (var c in originalComplexArrays)
            {
                transientComplexArrays.Add(new Complex[binCount]);
            }
            int[] flagSumArray = new int[originalComplexArrays.Count];
            for (int binIndex = 0; binIndex < binCount; binIndex++)
            {
                token.ThrowIfCancellationRequested();
                var currentBinArray = (from c in originalComplexArrays select c[binIndex]).ToArray();
                float[] fValues = F(currentBinArray, adjacencyNumber);
                float[] thresholdValues = Threshold(fValues, adjacencyNumber, strength);
                int[] currentFlags = fValues.Zip(thresholdValues, (f, t) => Convert.ToInt32(Flag(f, t))).ToArray();
                flagSumArray = flagSumArray.Zip(currentFlags, (a, b) => a + b).ToArray();
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            for (int i = 0; i < originalComplexArrays.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                if (IsTransientSample(flagSumArray[i], binCount, flagRatio))
                {
                    transientComplexArrays[i] = (Complex[])originalComplexArrays[i].Clone();
                    originalComplexArrays[i] = new Complex[transientComplexArrays[i].Length];
                }
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            return transientComplexArrays;
        }

        #endregion

        /// <summary>
        /// Gets the complex stft array of a given <see cref="Data.Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Data.Wave"/>.</param>
        /// <param name="samplingLength">The sampling length.</param>
        /// <param name="hopSize">The hop size.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns>The list of <see cref="Complex[]"/> containing the stft of the given <see cref="Data.Wave"/>.</returns>
        private static List<Complex[]> GetComplexArraysFromWave(Wave wave, int samplingLength, int hopSize, CancellationToken token)
        {
            double percentRatio = 1.0 / (int)Math.Ceiling((double)wave.Data.Length / hopSize);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= percentRatio;
            var fftBinArrays = new List<Complex[]>();
            for (int i = 0; i < wave.Data.Length; i += hopSize)
            {
                token.ThrowIfCancellationRequested();
                float[] sample = Computer.HannWindowCache.Apply(wave.Data.GetSample(i, samplingLength), WindowingType.Full);
                fftBinArrays.Add(Computer.FourierTransform(sample.ToComplexArray().ZeroPad(), token, false));
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= percentRatio;
            return fftBinArrays;
        }

        /// <summary>
        /// Calculates the transient and the residual <see cref="Data.Wave"/> using the stft <see cref="Complex"/> arrays. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Data.Wave"/>.</param>
        /// <param name="residualComplexArrays">The residual stft <see cref="Complex"/> arrays.</param>
        /// <param name="transientComplexArrays">The transient stft <see cref="Complex"/> arrays.</param>
        /// <param name="samplingLength">The sampling length.</param>
        /// <param name="hopSize">The hop size.</param>
        /// <param name="hopScale">The hop scale.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        private void CalculateTransientWave(Wave wave, List<Complex[]> residualComplexArrays, List<Complex[]> transientComplexArrays, int samplingLength, int hopSize, int hopScale, CancellationToken token)
        {
            double percentRatio = 1.0 / (int)Math.Ceiling((double)wave.Data.Length / hopSize);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= percentRatio;
            int binCount = residualComplexArrays[0].Length;
            float[] transientWave = new float[wave.Data.Length];
            float[] residualWave = new float[wave.Data.Length];
            for (int i = 0; i < residualComplexArrays.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                var residualSample = Computer.FourierTransform(residualComplexArrays[i], token, true)
                                             .InverseZeroPad(samplingLength)
                                             .Select(_ => (float)(_.Real / (binCount * hopScale)))
                                             .ToArray();
                var transientSample = Computer.FourierTransform(transientComplexArrays[i], token, true)
                                              .InverseZeroPad(samplingLength)
                                              .Select(_ => (float)(_.Real / (binCount * hopScale)))
                                              .ToArray();
                residualWave.AddSample(residualSample, i * hopSize, samplingLength);
                transientWave.AddSample(transientSample, i * hopSize, samplingLength);
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            wave.Data = residualWave;
            Wave = new Wave(transientWave, wave.WaveFormat);
            Wave.Resample(ProjectModel.StandardWaveFormat);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= percentRatio;
        }

        #endregion


        /// <summary>
        /// Transposes this <see cref="Transient"/> by shrinking or elongating it in the time-domain.
        /// </summary>
        /// <param name="freqScale">The frequency scale of the transposition.</param>
        private void Transpose(float freqScale)
        {
            int newLength = (int)(Wave.Data.Length / freqScale);
            double step = freqScale;
            double doubleIndex = 0;
            float[] newData = new float[newLength];
            for (int i = 0; i < newData.Length && Math.Ceiling(doubleIndex) < Wave.Data.Length; i++)
            {
                newData[i] = Computer.Lerp(Wave.Data[(int)Math.Floor(doubleIndex)], Wave.Data[(int)Math.Ceiling(doubleIndex)], (doubleIndex - Math.Floor(doubleIndex)) / step);
                doubleIndex += step;
            }
            Wave = new Wave(newData, Wave.WaveFormat);
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new Transient(this);
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        public Transient(bool isTransposable)
        {
            IsTransposable = isTransposable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class by making a deep copy of the given <see cref="Transient"/>.
        /// </summary>
        /// <param name="transient">The <see cref="Transient"/> to deep copy.</param>
        public Transient(Transient transient)
        {
            IsTransposable = transient.IsTransposable;
            Wave = new Wave(transient.Wave);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class by interpolating between two existing <see cref="Transient"/>s.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="a">The first <see cref="Transient"/>.</param>
        /// <param name="b">The second <see cref="Transient"/>.</param>
        /// <param name="lerpValue">The lerp value from a to b.</param>
        /// <param name="aFreqScale">The freqeuncy multiplier from <see cref="Note.NoteNumber"/> a to the current <see cref="Note.NoteNumber"/>.</param>
        /// <param name="bFreqScale">The freqeuncy multiplier from <see cref="Note.NoteNumber"/> b to the current <see cref="Note.NoteNumber"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Transient(Transient a, Transient b, double lerpValue, float aFreqScale, float bFreqScale, CancellationToken token)
        {
            var aCopy = new Transient(a);
            var bCopy = new Transient(b);
            if (aCopy.IsTransposable)
            {
                IsTransposable = true;
                aCopy.Transpose(aFreqScale);
            }
            if (bCopy.IsTransposable)
            {
                IsTransposable = true;
                bCopy.Transpose(bFreqScale);
            }
            if (a != b)
            {
                int aMaxIndex = aCopy.Wave.Data.AbsMaxIndex();
                int bMaxIndex = bCopy.Wave.Data.AbsMaxIndex();
                token.ThrowIfCancellationRequested();
                int maxIndex = Computer.Lerp(aMaxIndex, bMaxIndex, lerpValue);
                int length = Computer.Lerp(aCopy.Wave.Data.Length, bCopy.Wave.Data.Length, lerpValue);
                var shiftedAWaveData = new float[length];
                if (maxIndex > aMaxIndex)
                {
                    int start = maxIndex - aMaxIndex;
                    Array.Copy(aCopy.Wave.Data, 0, shiftedAWaveData, start, Math.Min(aCopy.Wave.Data.Length, length - start));
                }
                else
                {
                    int start = aMaxIndex - maxIndex;
                    Array.Copy(aCopy.Wave.Data, start, shiftedAWaveData, 0, Math.Min(aCopy.Wave.Data.Length - start, length));
                }
                token.ThrowIfCancellationRequested();
                var shiftedBWaveData = new float[length];
                if (maxIndex > bMaxIndex)
                {
                    int start = maxIndex - bMaxIndex;
                    Array.Copy(bCopy.Wave.Data, 0, shiftedBWaveData, start, Math.Min(bCopy.Wave.Data.Length, length - start));
                }
                else
                {
                    int start = bMaxIndex - maxIndex;
                    Array.Copy(bCopy.Wave.Data, start, shiftedBWaveData, 0, Math.Min(bCopy.Wave.Data.Length - start, length));
                }
                token.ThrowIfCancellationRequested();
                Wave = new Wave(shiftedAWaveData.Zip(shiftedBWaveData, (first, second) => Computer.Lerp(first, second, lerpValue)).ToArray(), a.Wave.WaveFormat);
            }
            else
            {
                Wave = new Wave(aCopy.Wave);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transient"/> class from a given <see cref="Data.Wave"/>. The <see cref="Data.Wave"/> is updated as the <see cref="Transient"/> is removed from it. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Data.Wave"/>.</param>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="hopScale">The hop scale.</param>
        /// <param name="strength">The strength of the detectable transient.</param>
        /// <param name="adjacencyNumber">The number of adjacent fft bins used in the calculation.</param>
        /// <param name="flagRatio">The minimum ratio of fft bins needed to flag a sample as a transient sample.</param>
        /// <param name="isTransposable">Indicates whether this transient should be transposable..</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Transient(Wave wave, float samplingFrequency, int hopScale, float strength, int adjacencyNumber, float flagRatio, bool isTransposable, CancellationToken token)
        {
            int sampleRate = wave.WaveFormat.SampleRate * wave.WaveFormat.Channels;
            int samplingLength = (int)(sampleRate / samplingFrequency);
            int hopSize = (int)(samplingLength / (2.0 * hopScale));
            IsTransposable = isTransposable;
            double loadingPercent = 0.35;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var originalComplexArrays = GetComplexArraysFromWave(wave, samplingLength, hopSize, token);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            loadingPercent = 0.3;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var transientComplexArrays = SeparateTransientFromWave(originalComplexArrays, strength, adjacencyNumber, flagRatio, token);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            loadingPercent = 0.35;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            CalculateTransientWave(wave, originalComplexArrays, transientComplexArrays, samplingLength, hopSize, hopScale, token);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
        }

        #endregion
    }
}
