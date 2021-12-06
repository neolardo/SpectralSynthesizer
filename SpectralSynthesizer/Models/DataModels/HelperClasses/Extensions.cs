using Newtonsoft.Json;
using SpectralSynthesizer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A static extension class for <see cref="List{T}"/>s.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Adds an item to this list in order. The order of the list is ascending by default.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="this">This list.</param>
        /// <param name="item">The new item to add.</param>
        /// <returns>The index of the inserted element.</returns>
        public static int AddInOrder<T>(this List<T> @this, T item) where T : IComparable<T>
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
                return 0;
            }
            if (@this[@this.Count - 1].CompareTo(item) <= 0)
            {
                @this.Add(item);
                return @this.Count - 1;
            }
            if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
                return 0;
            }
            int index = @this.BinarySearch(item);
            if (index < 0)
                index = ~index;
            @this.Insert(index, item);
            return index;
        }

        /// <summary>
        /// Gets the index of closest float element compared to the given float value in this ascendingly sorted list. 
        /// </summary>
        /// <param name="this">This ascendingly sorted list.</param>
        /// <param name="item">The serached element.</param>
        /// <param name="logLength">The log2 length of the list.</param>
        /// <returns>The index of the closest float element.</returns>
        public static int AscendingFloatBinarySearch(this List<float> @this, float item, int logLength)
        {
            float floatIndex = @this.Count / 2;
            float range = @this.Count / 2;
            int minIndex = (int)floatIndex;
            float minDistance = Math.Abs(@this[minIndex] - item);
            for (int step = 0; step < logLength; step++)
            {
                int intIndex = (int)floatIndex;
                if (Math.Abs(@this[intIndex] - item) < minDistance)
                {
                    minDistance = Math.Abs(@this[intIndex] - item);
                    minIndex = intIndex;
                }
                range /= 2f;
                if (@this[intIndex] > item)
                {
                    floatIndex -= range;
                }
                else
                {
                    floatIndex += range;
                }
            }
            return Math.Clamp(minIndex, 0, @this.Count - 1);
        }

        /// <summary>
        /// Gets the index of closest float element compared to the given float value in this descendingly sorted list. 
        /// </summary>
        /// <param name="this">This descendingly sorted list.</param>
        /// <param name="item">The serached element.</param>
        /// <param name="logLength">The log2 length of the list.</param>
        /// <returns>The index of the closest float element.</returns>
        public static int DescendingFloatBinarySearch(this List<float> @this, float item, int logLength)
        {
            float floatIndex = @this.Count / 2;
            float range = @this.Count / 2;
            int minIndex = (int)floatIndex;
            float minDistance = Math.Abs(@this[minIndex] - item);
            for (int step = 0; step < logLength; step++)
            {
                int intIndex = (int)floatIndex;
                if (Math.Abs(@this[intIndex] - item) < minDistance)
                {
                    minDistance = Math.Abs(@this[intIndex] - item);
                    minIndex = intIndex;
                }
                range /= 2f;
                if (@this[intIndex] > item)
                {
                    floatIndex += range;
                }
                else
                {
                    floatIndex -= range;
                }
            }
            return Math.Clamp(minIndex, 0, @this.Count - 1);
        }
    }

    /// <summary>
    /// A static extension class for <see cref="Array"/>s.
    /// </summary>
    public static class ArrayExtensions
    {
        #region Zero Pad

        /// <summary>
        /// Zero pads the input <see cref="Complex"/> array and returns it as a new array with a power of two length.
        /// </summary>
        /// <param name="this">The <see cref="Complex"/> array.</param>
        /// <returns>The zero padded <see cref="Complex"/> array.</returns>
        public static Complex[] ZeroPad(this Complex[] @this)
        {
            int n = @this.Length * 2;
            if ((n & (n - 1)) != 0) // not a power of two
            {
                n = (int)Math.Pow(2, Math.Ceiling(Math.Log2(n)));
            }
            var newArray = new Complex[n];
            int halfLength = @this.Length / 2;
            Array.Copy(@this, 0, newArray, n / 2 - halfLength, halfLength);
            Array.Copy(@this, halfLength, newArray, n / 2, halfLength);
            return newArray;
        }

        /// <summary>
        /// Inverts the zero pad done on this <see cref="Complex"/> array and returns it as a new array with the original length.
        /// </summary>
        /// <param name="this">The <see cref="Complex"/> array.</param>
        /// <param name="originalLength">The original length of the array.</param>
        /// <returns>The zero padded <see cref="Complex"/> array.</returns>
        public static Complex[] InverseZeroPad(this Complex[] @this, int originalLength)
        {
            var newArray = new Complex[originalLength];
            int halfLength = originalLength / 2;
            Array.Copy(@this, @this.Length / 2, newArray, halfLength, halfLength);
            Array.Copy(@this, @this.Length / 2 - halfLength, newArray, 0, halfLength);
            return newArray;
        }

        /// <summary>
        /// Zero pads the input <see cref="float"/> array and returns it as a new array with a power of two length.
        /// </summary>
        /// <param name="this">The <see cref="float"/> array.</param>
        /// <returns>The zero padded <see cref="float"/> array.</returns>
        public static float[] ZeroPad(this float[] @this)
        {
            int n = @this.Length;
            if ((n & (n - 1)) != 0) // not a power of two
            {
                n = (int)Math.Pow(2, Math.Ceiling(Math.Log2(n)));
            }
            var newArray = new float[n];
            Array.Copy(@this, 0, newArray, 0, @this.Length);
            return newArray;
        }

        #endregion

        #region Float To Complex

        /// <summary>
        /// Converts a <see cref="float"/> array into a <see cref="Complex"/> array.
        /// </summary>
        /// <param name="this">The <see cref="float"/> input array.</param>
        /// <returns>The <see cref="Complex"/> array output.</returns>
        public static Complex[] ToComplexArray(this float[] @this)
        {
            Complex[] output = new Complex[@this.Length];
            for (int i = 0; i < @this.Length; i++)
                output[i] = new Complex(@this[i], 0);
            return output;
        }

        /// <summary>
        /// Converts a <see cref="float"/> array into a <see cref="Complex"/> array.
        /// </summary>
        /// <param name="this">The <see cref="float"/> input array.</param>
        /// <param name="startIndex">The start index of the conversion.</param>
        /// <param name="endIndex">The end index of the conversion.</param>
        /// <returns>The <see cref="Complex"/> array output.</returns>
        public static Complex[] ToComplexArray(float[] @this, int startIndex, int endIndex)
        {
            Complex[] output = new Complex[endIndex - startIndex + 1];
            for (int i = startIndex; i <= endIndex; i++)
                output[i - startIndex] = new Complex(@this[i], 0);
            return output;
        }

        #endregion

        #region Max Index

        /// <summary>
        /// Gets the index of the absolute maximum in this array.
        /// </summary>
        /// <param name="this">This array.</param>
        /// <returns>The index of the absolute maximum.</returns>
        public static int AbsMaxIndex(this float[] @this)
        {
            float max = MathF.Abs(@this[0]);
            int maxInd = 0;
            for (int i = 1; i < @this.Length; i++)
            {
                if (Math.Abs(@this[i]) > max)
                {
                    maxInd = i;
                    max = Math.Abs(@this[i]);
                }
            }
            return maxInd;
        }

        #endregion

        #region Audio Manipulations

        #region Sampling

        /// <summary>
        /// Gets a sample from this float array.
        /// </summary>
        /// <param name="this">This float array.</param>
        /// <param name="center">The center of the sample.</param>
        /// <param name="length">The length of the sample.</param>
        /// <returns></returns>
        public static float[] GetSample(this float[] @this, int center, int length)
        {
            float[] sample = new float[length];
            int halfLength = length / 2;
            int sampleStart = Computer.ClampMin(center - halfLength, 0);
            int sampleEnd = Computer.ClampMax(center + halfLength, @this.Length);
            Array.Copy(@this, sampleStart, sample, halfLength - (center - sampleStart), center - sampleStart);
            Array.Copy(@this, center, sample, halfLength, sampleEnd - center);
            return sample;
        }

        /// <summary>
        /// Sums up the given sample with this array's elements at the given center point.
        /// </summary>
        /// <param name="this">This float array.</param>
        /// <param name="sample">The sample to add.</param>
        /// <param name="center">The index of this array, where the sample should be centered.</param>
        /// <param name="length">The maximum length of the sample.</param>
        public static void AddSample(this float[] @this, float[] sample, int center, int length)
        {
            int sampleStart = Computer.ClampMin(center - length / 2, 0);
            int sampleEnd = Computer.ClampMax(center + length / 2, @this.Length);
            int offset = (length / 2) - (center - sampleStart);
            for (int j = 0; j < sampleEnd - sampleStart; j++)
            {
                @this[j + sampleStart] += sample[j + offset];
            }
        }

        #endregion

        #region Fading

        /// <summary>
        /// Fades in and out this <see cref="float[]"/>.
        /// </summary>
        /// <param name="this">This <see cref="float[]"/>.</param>
        /// <returns>The faded <see cref="float[]"/>.</returns>
        public static float[] Fade(this float[] @this)
        {
            @this.FadeIn(ProjectModel.FadeSampleNumber);
            @this.FadeOut(ProjectModel.FadeSampleNumber);
            return @this;
        }

        /// <summary>
        /// Fades out this <see cref="float[]"/>.
        /// </summary>
        /// <param name="this">This <see cref="float[]"/>.</param>
        /// <param name="sampleNumber">The number of samples to fade in.</param>
        /// <returns>The faded <see cref="float[]"/>.</returns>
        public static float[] FadeIn(this float[] @this, int sampleNumber = ProjectModel.FadeSampleNumber)
        {
            int length = Math.Min(@this.Length, sampleNumber);
            for (int i = 0; i < length; i++)
            {
                @this[i] = @this[i] * (i / (float)length);
            }
            return @this;
        }

        /// <summary>
        /// Fades out this <see cref="float[]"/>.
        /// </summary>
        /// <param name="this">This <see cref="float[]"/>.</param>
        /// <param name="sampleNumber">The number of samples to fade out.</param>
        /// <returns>The faded <see cref="float[]"/>.</returns>
        public static float[] FadeOut(this float[] @this, int sampleNumber = ProjectModel.FadeSampleNumber)
        {
            int end = Math.Max(@this.Length - sampleNumber, 0);
            for (int i = @this.Length - 1; i >= end; i--)
            {
                @this[i] = @this[i] * (1f - ((i - end) / (float)(@this.Length - end)));
            }
            return @this;
        }

        #endregion

        #region Normalizing

        /// <summary>
        /// Normalizes the given float wave.
        /// </summary>
        /// <param name="this">This float wave.</param>
        /// <param name="maxAmplitude">The maximum desired amplitude.</param>
        /// <returns>The normalized result wave.</returns>
        public static float[] Normalize(this float[] @this, float maxAmplitude = 1.0f)
        {
            float globalMax = @this.Select(_ => MathF.Abs(_)).Max();
            float ratio = maxAmplitude / globalMax;
            return @this.Select(_ => _ * maxAmplitude).ToArray();
        }

        #endregion

        #endregion

        #region Write To Json

        /// <summary>
        /// Write this float array to the given ,json file.
        /// </summary>
        /// <param name="this">The <see cref="float"/> array.</param>
        /// <param name="filePath">The fullpath of the .json file.</param>
        /// <returns>The zero padded <see cref="float"/> array.</returns>
        public static void WriteJson(this float[] @this, string filePath)
        {
            string textoutput = JsonConvert.SerializeObject(
               @this,
               Formatting.Indented, new JsonSerializerSettings
               {
                   PreserveReferencesHandling = PreserveReferencesHandling.None,
                   TypeNameHandling = TypeNameHandling.Auto
               });
            File.WriteAllText(filePath, textoutput);
        }


        #endregion
    }

    /// <summary>
    /// A static extension class for <see cref="Enum"/>s.
    /// </summary>
    public static class EnumExtensions
    {
        #region Instrument Model Type

        /// <summary>
        /// Counts the flags of this <see cref="InstrumentModelType"/>.
        /// </summary>
        /// <param name="this">This <see cref="InstrumentModelType"/>.</param>
        /// <returns>The number of models this <see cref="InstrumentModelType"/> consists of.</returns>
        public static int Count(this InstrumentModelType @this)
        {
            int value = (int)@this;
            int count = 0;
            while (value != 0)
            {
                if ((value & 1) != 0) count++;
                value = value >> 1;
            }
            return count;
        }

        #endregion
    }
}
