using NAudio.Wave;
using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using System;
using System.Numerics;
using System.Threading;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A static class to manage and compute any kind of audio data.
    /// </summary>
    public static class Computer
    {
        #region Properties

        /// <summary>
        /// The delta used for floating point comparing.
        /// </summary>
        public static double CompareDelta => 0.0001;

        /// <summary>
        /// The lenght of any <see cref="NumericCache"/>.
        /// </summary>
        private static int StandardNumericCacheLength => 10000;

        /// <summary>
        /// The length of any buffer that is used for audio rendering.
        /// </summary>
        private static int AudioRenderingBufferLength => ProjectModel.StandardSampleRate / 2;

        /// <summary>
        /// A random instance used in random calculations.
        /// </summary>
        public static Random R { get; } = new Random();

        #region Cancellable Tasks

        /// <summary>
        /// A <see cref="CancellableTask"/> for <see cref="Wave"/> to <see cref="Midi"/> conversion.
        /// </summary>
        private static CancellableTask WaveToMidiConversionTask { get; set; } = null;

        /// <summary>
        /// A <see cref="CancellableTask"/> for generating a new <see cref="Instrument"/>.
        /// </summary>
        private static CancellableTask InstrumentGenerationTask { get; set; } = null;

        /// <summary>
        /// A <see cref="CancellableTask"/> for generating a new <see cref="Note"/>.
        /// </summary>
        private static CancellableTask NoteGenerationTask { get; set; } = null;

        /// <summary>
        /// A <see cref="CancellableTask"/> for rendering audio.
        /// </summary>
        private static CancellableTask AudioRenderingTask { get; set; } = null;

        #endregion

        #region Caches

        /// <summary>
        /// The decibel cache.
        /// </summary>
        private static DecibelCache DecibelCache { get; } = new DecibelCache(ProjectModel.DecibelCacheLocation, ProjectModel.GlobalMinimumDecibel, 100);

        /// <summary>
        /// The discrete frequency cache.
        /// </summary>
        private static DiscreteFrequencyCache DiscreteFrequencyCache { get; } = new DiscreteFrequencyCache(ProjectModel.FrequencyCacheLocation);

        /// <summary>
        /// The logarithm cache used for logarithmic frequency scaling.
        /// </summary>
        private static LogarithmCache LogarithmCache { get; } = new LogarithmCache(ProjectModel.LogarithmCacheLocation, MathF.Pow(2f, 1f / 12f), 1, ProjectModel.StandardSampleRate, 100);

        /// <summary>
        /// The sine wave cache used for sinusoid generation.
        /// </summary>
        public static SineWaveCache SineWaveCache { get; } = new SineWaveCache(ProjectModel.SineWaveCacheLocation, StandardNumericCacheLength);

        /// <summary>
        /// The Hann window cache used for windowing.
        /// </summary>
        public static HannWindowCache HannWindowCache { get; } = new HannWindowCache(ProjectModel.HannWindowCacheLocation, StandardNumericCacheLength);

        #endregion

        #endregion

        #region Methods

        #region Cache Loading

        /// <summary>
        /// Loads the caches.
        /// </summary>
        public static void LoadCaches()
        {
            DecibelCache.LoadCache();
            DiscreteFrequencyCache.LoadCache();
            LogarithmCache.LoadCache();
            SineWaveCache.LoadCache();
            HannWindowCache.LoadCache();
        }

        #endregion

        #region Mathematical Methods

        #region Fourier Transform

        /* 
         * Free FFT and convolution (C#)
         * 
         * Copyright (c) 2020 Project Nayuki. (MIT License)
         * https://www.nayuki.io/page/free-small-fft-in-multiple-languages
         * 
         * Permission is hereby granted, free of charge, to any person obtaining a copy of
         * this software and associated documentation files (the "Software"), to deal in
         * the Software without restriction, including without limitation the rights to
         * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
         * the Software, and to permit persons to whom the Software is furnished to do so,
         * subject to the following conditions:
         * - The above copyright notice and this permission notice shall be included in
         *   all copies or substantial portions of the Software.
         * - The Software is provided "as is", without warranty of any kind, express or
         *   implied, including but not limited to the warranties of merchantability,
         *   fitness for a particular purpose and noninfringement. In no event shall the
         *   authors or copyright holders be liable for any claim, damages or other
         *   liability, whether in an action of contract, tort or otherwise, arising from,
         *   out of or in connection with the Software or the use or other dealings in the
         *   Software.
         */

        /// <summary>
        /// Computes and returns the discrete Fourier transform (DFT) or inverse transform of the given <see cref="Complex"/> vector.
        /// </summary>
        /// <param name="input">The <see cref="Complex"/> input array.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="inverse">True if the transformation is inverse.</param>
        /// <returns>The discrete Fourier transform (DFT) or inverse transform of the given <see cref="Complex"/> vector.</returns>
        public static Complex[] FourierTransform(Complex[] input, CancellationToken token, bool inverse = false)
        {
            if ((input.Length & (input.Length - 1)) != 0) // not a power of two
            {
                throw new Exception("Every FFT input should have a power of 2 length.");
            }
            var output = new Complex[input.Length];
            input.CopyTo(output, 0);
            FourierTransform(output, inverse, token);
            return output;
        }

        /// <summary>
        /// Computes the discrete Fourier transform (DFT) or inverse transform of the given complex vector, storing the result back into the vector.
        /// The vector can have any length. This is a wrapper function. The inverse transform does not perform scaling, so it is not a true inverse.
        /// </summary>
        /// <param name="vec">The <see cref="Complex"/> array input.</param>
        /// <param name="inverse">True if the transformation is inverse.</param>
        /// <param name="token">The cancellation token.</param>
        private static void FourierTransform(Complex[] vec, bool inverse, CancellationToken token)
        {
            if (vec.Length == 0)
                return;
            FourierTransformRadix2(vec, inverse, token);
        }

        /// <summary>
        /// Computes the discrete Fourier transform(DFT) of the given complex vector, storing the result back into the vector.
        /// The vector's length must be a power of 2. Uses the Cooley-Tukey decimation-in-time radix-2 algorithm.
        /// </summary>
        /// <param name="vec">The complex number input array.</param>
        /// <param name="inverse">Indicates whether the transformation should be reversed or not.</param>
        /// <param name="token">The cancellation token.</param>
        private static void FourierTransformRadix2(Complex[] vec, bool inverse, CancellationToken token)
        {
            // Length variables
            int n = vec.Length;
            int levels = 0;  // compute levels = floor(log2(n))
            for (int temp = n; temp > 1; temp >>= 1)
                levels++;
            if (1 << levels != n)
                throw new ArgumentException("Length is not a power of 2");

            // Trigonometric table
            Complex[] expTable = new Complex[n / 2];
            float coef = (float)(2 * Math.PI / n * (inverse ? 1 : -1));
            for (int i = 0; i < n / 2; i++)
                expTable[i] = Complex.FromPolarCoordinates(1.0f, i * coef);

            // Bit-reversed addressing permutation
            for (int i = 0; i < n; i++)
            {
                int j = ReverseBits(i, levels);
                if (j > i)
                {
                    Complex temp = vec[i];
                    vec[i] = vec[j];
                    vec[j] = temp;
                }
            }

            // Cooley-Tukey decimation-in-time radix-2 FFT
            for (int size = 2; size <= n; size *= 2)
            {
                token.ThrowIfCancellationRequested();

                int halfsize = size / 2;
                int tablestep = n / size;
                for (int i = 0; i < n; i += size)
                {
                    for (int j = i, k = 0; j < i + halfsize; j++, k += tablestep)
                    {
                        Complex temp = vec[j + halfsize] * expTable[k];
                        vec[j + halfsize] = vec[j] - temp;
                        vec[j] += temp;
                    }
                }
                if (size == n)  // Prevent overflow in 'size *= 2'
                    break;
            }
        }

        /// <summary>
        /// Computes the discrete Fourier transform (DFT) of the given complex vector, storing the result back into the vector.
        /// The vector can have any length. This requires the convolution function, which in turn requires the radix-2 FFT function.
        /// Uses Bluestein's chirp z-transform algorithm.
        /// </summary>
        /// <param name="vec">The complex number input array.</param>
        /// <param name="inverse">Indicates whether the transformation should be reversed or not.</param>
        /// <param name="token">The cancellation token.</param>
        private static void FourierTransformBluestein(Complex[] vec, bool inverse, CancellationToken token)
        {
            // Find a power-of-2 convolution length m such that m >= n * 2 + 1
            int n = vec.Length;
            if (n >= 0x20000000)
                throw new ArgumentException("Array too large");
            int m = 1;
            while (m < n * 2 + 1)
                m *= 2;

            // Trignometric table
            Complex[] expTable = new Complex[n];
            float coef = (float)(Math.PI / n * (inverse ? 1 : -1));
            for (int i = 0; i < n; i++)
            {
                int j = (int)((long)i * i % (n * 2));  // This is more accurate than j = i * i
                expTable[i] = Complex.Exp(new Complex(0, j * coef));
            }

            token.ThrowIfCancellationRequested();

            // Temporary vectors and preprocessing
            Complex[] avec = new Complex[m];
            for (int i = 0; i < n; i++)
                avec[i] = vec[i] * expTable[i];

            token.ThrowIfCancellationRequested();

            Complex[] bvec = new Complex[m];
            bvec[0] = expTable[0];
            for (int i = 1; i < n; i++)
                bvec[i] = bvec[m - i] = Complex.Conjugate(expTable[i]);

            token.ThrowIfCancellationRequested();

            // Convolution
            Complex[] cvec = new Complex[m];
            Convolve(avec, bvec, cvec, token);

            token.ThrowIfCancellationRequested();

            // Postprocessing
            for (int i = 0; i < n; i++)
                vec[i] = cvec[i] * expTable[i];
        }

        /// <summary>
        /// Computes the circular convolution of the given complex vectors.Each vector's length must be the same.
        /// </summary>
        /// <param name="xvec">The first complex number array.</param>
        /// <param name="yvec">The second complex number array.</param>
        /// <param name="outvec">The result complex number array.</param>
        /// <param name="token">The cancellation token.</param>
        private static void Convolve(Complex[] xvec, Complex[] yvec, Complex[] outvec, CancellationToken token)
        {
            int n = xvec.Length;
            if (n != yvec.Length || n != outvec.Length)
                throw new ArgumentException("Mismatched lengths");
            xvec = (Complex[])xvec.Clone();
            yvec = (Complex[])yvec.Clone();
            FourierTransform(xvec, false, token);
            FourierTransform(yvec, false, token);
            for (int i = 0; i < n; i++)
                xvec[i] *= yvec[i];
            FourierTransform(xvec, true, token);
            for (int i = 0; i < n; i++)  // Scaling (because this FFT implementation omits it)
                outvec[i] = xvec[i] / n;
        }

        /// <summary>
        /// Reverses the bits of the given integer value.
        /// </summary>
        /// <param name="val">The integer value which's bits are reversed.</param>
        /// <param name="range">The range in where the bits are reversed.</param>
        /// <returns></returns>
        private static int ReverseBits(int val, int range)
        {
            int result = 0;
            for (int i = 0; i < range; i++, val >>= 1)
                result = (result << 1) | (val & 1);
            return result;
        }

        #endregion

        #region Parabolic Fitting

        /// <summary>
        /// Gets the parabola's parameter which is defined by the given three magnitudes.
        /// </summary>
        /// <param name="a">The left magnitude from the center.</param>
        /// <param name="b">The center magnitde.</param>
        /// <param name="c">The right magnitude from the center.</param>
        /// <returns>The parabola's vertex's X coordinate and the <see cref="b"/> magnitude's difference.</returns>
        /// <exception cref="ArithmeticException"></exception>
        public static double GetParabolicParameterFromThreeMagnitudes(double a, double b, double c)
        {
            return 0.5 * (a - c) / (a - 2 * b + c);
        }

        #endregion

        #region Greatest Common Divisor

        /// <summary>
        /// Gets the greatest common divisor of the given numbers.
        /// </summary>
        /// <param name="a">The first number.</param>
        /// <param name="b">The second number.</param>
        /// <returns>The greatest common divisor of the given numbers.</returns>
        public static int GetGreatestCommonDivisor(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a | b;
        }

        #endregion

        #region Linear Interpolation

        /// <summary>
        /// Return a linearly interpolated value between two doubles.
        /// </summary>
        /// <param name="start">The starting double value.</param>
        /// <param name="end">The end double value.</param>
        /// <param name="value">The lerping value.</param>
        /// <returns>The linearly interpolated value between the two given doubles.</returns>
        public static double Lerp(double start, double end, double value)
        {
            if (value < 0.0)
                return start;
            else if (value > 1.0)
                return end;
            else
                return start * (1.0 - value) + end * value;
        }


        /// <summary>
        /// Return a linearly interpolated value between two floats.
        /// </summary>
        /// <param name="start">The starting float value.</param>
        /// <param name="end">The end float value.</param>
        /// <param name="value">The lerping value.</param>
        /// <returns>The linearly interpolated value between the two given floats.</returns>
        public static float Lerp(float start, float end, double value)
        {
            if (value < 0.0)
                return start;
            else if (value > 1.0)
                return end;
            else
                return (float)(start * (1.0 - value) + end * value);
        }

        /// <summary>
        /// Return the linearly interpolated value between two integers.
        /// </summary>
        /// <param name="start">The starting integer value.</param>
        /// <param name="end">The end integer value.</param>
        /// <param name="value">The lerping value.</param>
        /// <returns>The linearly interpolated value between the two given integers.</returns>
        public static int Lerp(int start, int end, double value)
        {
            if (value < 0.0)
                return start;
            else if (value > 1.0)
                return end;
            else
                return (int)(start * (1.0 - value) + end * value);
        }

        #endregion

        #region Clamping

        #region Max

        /// <summary>
        /// Caps the given double value if it is larger than the maximum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The maximum if the value is larger than it, otherwise the value.</returns>
        public static double ClampMax(double value, double maximum)
        {
            return value > maximum ? maximum : value;
        }

        /// <summary>
        /// Clamps the given float value if it is larger than the maximum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The maximum if the value is larger than it, otherwise the value.</returns>
        public static float ClampMax(float value, float maximum)
        {
            return value > maximum ? maximum : value;
        }

        /// <summary>
        /// Caps the given integer value if it is larger than the maximum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="maximum">The maximum.</param>
        /// <returns>The maximum if the value is larger than it, otherwise the value.</returns>
        public static int ClampMax(int value, int maximum)
        {
            return value > maximum ? maximum : value;
        }

        #endregion

        #region Min

        /// <summary>
        /// Clamps the given double value if it is smaller than the minimum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minimum">The minimum.</param>
        /// <returns>The minimum if the value is smaller than it, otherwise the value.</returns>
        public static double ClampMin(double value, double minimum)
        {
            return value < minimum ? minimum : value;
        }

        /// <summary>
        /// Clamps the given float value if it is smaller than the minimum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minimum">The minimum.</param>
        /// <returns>The minimum if the value is smaller than it, otherwise the value.</returns>
        public static float ClampMin(float value, float minimum)
        {
            return value < minimum ? minimum : value;
        }

        /// <summary>
        /// Caps the given integer value if it is smaller than the minimum.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="minimum">The minimum.</param>
        /// <returns>The minimum if the value is smaller than it, otherwise the value.</returns>
        public static int ClampMin(int value, int minimum)
        {
            return value < minimum ? minimum : value;
        }

        #endregion

        #endregion

        #endregion

        #region Conversions

        #region Amplitude Conversions

        /// <summary>
        /// Gets the decibel value from a given PCM amplitude.
        /// </summary>
        /// <param name="pcmAmplitude">The PCM amplitude.</param>
        /// <returns>The frequency.</returns>
        public static float PCMAmplitudeToDecibel(float pcmAmplitude) => DecibelCache.GetDecibelOf(pcmAmplitude);

        /// <summary>
        /// Gets the PCM amplitude from a given decibel value.
        /// </summary>
        /// <param name="decibel">The decibel value.</param>
        /// <returns>The PCM amplitude.</returns>
        public static float DecibelToPCMAmplitude(float decibel) => DecibelCache.GetPCMOf(decibel);

        #endregion

        #region Frequency Conversions

        #region To Frequency

        #region Discrete Frequency

        /// <summary>
        /// Gets the frequency from a given discrete frequency number.
        /// </summary>
        /// <param name="discreteFrequency">The discrete frequency.</param>
        /// <returns>The frequency.</returns>
        public static float DiscreteFrequencyToFrequency(int discreteFrequency) => DiscreteFrequencyCache.GetFrequency(discreteFrequency);

        /// <summary>
        /// Gets the frequency from a given note number.
        /// </summary>
        /// <param name="noteNumber">The note number.</param>
        /// <returns>The frequency.</returns>
        public static float NoteToFrequency(int noteNumber) => DiscreteFrequencyToFrequency(noteNumber * ProjectModel.TonePerNote);

        #endregion

        #region Logarithmic Frequency

        /// <summary>
        /// Gets the frequency's value from it's the base 12 logarithm.
        /// </summary>
        /// <param name="logarithmicFrequency">The base 12 logarithm of the frequency.</param>
        /// <returns>The frequency.</returns>
        public static float LogarithmicFrequencyToFrequency(float logarithmicFrequency) => LogarithmCache.GetValueOf(logarithmicFrequency);

        #endregion

        #endregion

        #region From Frequency

        #region Discrete Frequency

        /// <summary>
        /// Converts a given frequency value to a discrete frequency.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <returns>The corresponding discrete frequency.</returns>
        public static int FrequencyToDiscreteFrequency(float frequency) => DiscreteFrequencyCache.GetDiscreteFrequency(frequency);

        /// <summary>
        /// Converts a given frequency value to a note number.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <returns>The note number.</returns>
        public static int FrequencyToNote(float frequency) => FrequencyToDiscreteFrequency(frequency) / ProjectModel.TonePerNote;

        #endregion

        #region Logarithmic Frequency

        /// <summary>
        /// Converts a given frequency value to a base 12 logarithmic value.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <returns>The base 12 logarithm of the frequency.</returns>
        public static float FrequencyToLogarithmicFrequency(float frequency) => LogarithmCache.GetLogarithmOf(frequency);

        #endregion

        #region String

        /// <summary>
        /// Converts an int whole note to a string. 
        /// Ex: 0 = 'c0', 13 = 'c#1'.
        /// </summary>
        /// <param name="note">The note's whole note number</param>
        /// <returns>The string representation of the note</returns>
        public static string NoteToString(int note)
        {
            string n = "";
            switch (note % 12)
            {
                case 0: n = "c"; break;
                case 1: n = "c#"; break;
                case 2: n = "d"; break;
                case 3: n = "d#"; break;
                case 4: n = "e"; break;
                case 5: n = "f"; break;
                case 6: n = "f#"; break;
                case 7: n = "g"; break;
                case 8: n = "g#"; break;
                case 9: n = "a"; break;
                case 10: n = "a#"; break;
                case 11: n = "h"; break;
            }
            n += (note / 12).ToString();
            return n;
        }

        /// <summary>
        /// Converts a string note to an int discrete frequency
        /// Ex: 0 = 'c0', 13 = 'c#1'.
        /// </summary>
        /// <param name="note">The string representation of the note</param>
        /// <returns>The discrete frequency of the note</returns>
        public static int StringToNote(string note)
        {
            if (note.Length > 3 || note.Length < 2)
                throw new InvalidNoteFormatException();
            int n = 0;
            bool cansharp = false;
            switch (note[0])
            {
                case 'c':
                    n = 0;
                    cansharp = true;
                    break;
                case 'd':
                    n = 2;
                    cansharp = true;
                    break;
                case 'e':
                    n = 4;
                    break;
                case 'f':
                    n = 5;
                    cansharp = true;
                    break;
                case 'g':
                    n = 7;
                    cansharp = true;
                    break;
                case 'a':
                    n = 9;
                    cansharp = true;
                    break;
                case 'h':
                    n = 11;
                    break;
                default:
                    throw new InvalidNoteFormatException();
            }
            int octave_ind = 1;
            if (note[1] == '#')
            {
                if (cansharp == false)
                    throw new InvalidNoteFormatException();
                octave_ind = 2;
                n += 1;
            }
            int octave = 0;
            if (note.Length != octave_ind + 1)
                throw new InvalidNoteFormatException();

            if (int.TryParse(note[octave_ind].ToString(), out octave))
            {
                int result = octave * 12 + n;
                if (result < 0)
                    throw new InvalidNoteFormatException();
                return result;
            }
            else
                throw new InvalidNoteFormatException();
        }

        #endregion

        #endregion

        #endregion

        #endregion

        #region Task Functions

        #region Wave to Midi Conversion

        /// <summary>
        /// Starts the <see cref="Wave"/> to <see cref="Midi"/> conversion on a <see cref="CancellableTask"/>.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        /// <param name="selection">The start and end ratios of the <see cref="Wave"/>'s length indicating the relevant part of the conversion.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any possible note to be noticed.</param>
        /// <param name="minimumLength">The minimum length of any possible note in milliseconds.</param>
        /// <param name="maximumSilence">The maximum amount of silence in any possible note in milliseconds.</param>
        public static void StartWaveToMidiConversionTask(Wave wave, (double start, double end) selection, float minimumDecibel, int minimumLength, int maximumSilence)
        {
            CancelWaveToMidiConversionTask();
            WaveToMidiConversionTask?.Wait();
            WaveToMidiConversionTask = new CancellableTask();
            Action action = () =>
            {
                Midi midi = null;
                try
                {
                    midi = new Midi(wave, selection, minimumDecibel, minimumLength, maximumSilence, WaveToMidiConversionTask.Token);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnEndWaveToMidiConversion(midi);
                    }));
                }
                catch (OperationCanceledException)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnCancelWaveToMidiConversion();
                    }));
                    return;
                }

            };
            WaveToMidiConversionTask.Start(action);
        }

        /// <summary>
        /// Aborts the <see cref="Wave"/> to <see cref="Midi"/> transformation.
        /// </summary>
        public static void CancelWaveToMidiConversionTask()
        {
            WaveToMidiConversionTask?.Cancel();
        }

        #endregion

        #region Instrument Generation

        /// <summary>
        /// Starts the <see cref="Instrument"/> generation on a <see cref="CancellableTask"/>.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/> with sound of the <see cref="Instrument"/>.</param>
        /// <param name="midi">The <see cref="Midi"/> with the notes aligned to the <see cref="Wave"/>'s notes.</param>
        /// <param name="selection">The start and end ratios of the <see cref="Wave"/>'s length indicating the relevant part of the generation.</param>.
        /// <param name="type">The type of models used in the generation.</param>
        /// <param name="sinusoidParameters">The <see cref="SinusoidParameters"/>.</param>
        /// <param name="transientParameters">The <see cref="TransientParameters"/>.</param>
        /// <param name="noiseParameters">The <see cref="NoiseParameters"/>.</param>
        public static void StartInstrumentGenerationTask(Wave wave, Midi midi, (double start, double end) selection, InstrumentModelType type, SinusoidParameters sinusoidParameters, TransientParameters transientParameters, NoiseParameters noiseParameters)
        {
            CancelInstrumentGenerationTask();
            InstrumentGenerationTask?.Wait();
            InstrumentGenerationTask = new CancellableTask();
            Action action = () =>
            {
                Instrument instrument = null;
                try
                {
                    instrument = new Instrument(wave, midi, selection, type, sinusoidParameters, transientParameters, noiseParameters, InstrumentGenerationTask.Token);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnEndInstrumentGeneration(instrument);
                    }));
                }
                catch (OperationCanceledException)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnCancelInstrumentGeneration();
                    }));
                    return;
                }
            };
            InstrumentGenerationTask.Start(action);
        }

        /// <summary>
        /// Aborts the <see cref="Instrument"/> generation.
        /// </summary>
        public static void CancelInstrumentGenerationTask()
        {
            InstrumentGenerationTask?.Cancel();
        }

        #endregion

        #region Note Generation

        /// <summary>
        /// Starts the <see cref="Note"/> generation process on a <see cref="CancellableTask"/>.
        /// </summary>
        /// <param name="instrument">The <see cref="Instrument"/>.</param>
        /// <param name="noteNumber">The note number of the new <see cref="Note"/>.</param>
        public static void StartNoteGenerationTask(Instrument instrument, int noteNumber)
        {
            CancelNoteGenerationTask();
            NoteGenerationTask?.Wait();
            NoteGenerationTask = new CancellableTask();
            Action action = () =>
            {
                try
                {
                    var note = instrument.GetNoteAt(noteNumber, NoteGenerationTask.Token);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnEndNoteGeneration(note);
                    }));
                }
                catch (OperationCanceledException)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<InstrumentBuilderViewModel>().OnCancelNoteGeneration();
                    }));
                    return;
                }
            };
            NoteGenerationTask.Start(action);
        }

        /// <summary>
        /// Aborts the <see cref="Instrument"/> generation.
        /// </summary>
        public static void CancelNoteGenerationTask()
        {
            NoteGenerationTask?.Cancel();
        }

        #endregion

        #region Audio Rendering

        /// <summary>
        /// Starts the audio rendering task.
        /// </summary>
        /// <param name="renderable">The <see cref="IRenderable"/> object.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="buffer">The <see cref="AudioBufferWaveProvider"/> storing the <see cref="IRenderable"/>'s audio data.</param>
        public static void StartAudioRenderingTask(IRenderable renderable, int sampleRate, AudioBufferWaveProvider buffer)
        {
            CancelAudioRenderingTask();
            AudioRenderingTask?.Wait();
            AudioRenderingTask = new CancellableTask();
            Action action = () =>
            {
                try
                {
                    while (buffer.IsBufferingComplete == false)
                    {
                        buffer.InsertData(renderable.Render(sampleRate, buffer.Length, AudioRenderingTask.Token), 0);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            };
            AudioRenderingTask.Start(action);
        }

        /// <summary>
        /// Starts the <see cref="Wave"/> audio rendering task.
        /// </summary>
        /// <param name="midi">The <see cref="Midi"/>.</param>
        /// <param name="instrument">The <see cref="Instrument"/>.</param>
        /// <param name="selection">The selected part of the <see cref="Midi"/>.</param>
        /// <param name="enableSustain">Indicates whether sustain mode is enabled.</param>
        /// <param name="waveFormat">The wave format.</param>
        public static void StartWaveAudioRenderingTask(Midi midi, Instrument instrument, (double Start, double End) selection, bool enableSustain, WaveFormat waveFormat)
        {
            CancelAudioRenderingTask();
            AudioRenderingTask?.Wait();
            AudioRenderingTask = new CancellableTask();
            Action action = () =>
            {
                try
                {
                    var wave = new Wave(midi, instrument, selection, enableSustain, waveFormat, AudioRenderingTask.Token);
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<AudioRendererViewModel>().OnEndAudioRendering(wave);
                    }));
                }
                catch (OperationCanceledException)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        IoC.Get<AudioRendererViewModel>().OnCancelAudioRendering();
                    }));
                    return;
                }
            };
            AudioRenderingTask.Start(action);
        }


        /// <summary>
        /// Aborts the audio rendering.
        /// </summary>
        public static void CancelAudioRenderingTask()
        {
            AudioRenderingTask?.Cancel();
        }

        #endregion

        /// <summary>
        /// Cancels all the currently running tasks managed by this class.
        /// </summary>
        public static void CancelEverything()
        {
            CancelWaveToMidiConversionTask();
            CancelInstrumentGenerationTask();
            CancelNoteGenerationTask();
            CancelAudioRenderingTask();
        }

        #endregion

        #endregion
    }
}