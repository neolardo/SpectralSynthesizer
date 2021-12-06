using System;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A cache storing Hann-window values.
    /// </summary>
    public class HannWindowCache : NumericCache
    {
        #region Properties

        /// <summary>
        /// The Hann window values.
        /// </summary>
        private float[] Values { get; set; }

        /// <summary>
        /// The normlaized Hann window's integral values.
        /// </summary>
        private float[] IntegralValues { get; set; }

        /// <summary>
        /// The length of the <see cref="Values"/> array.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The Hann window's effective range's offset ratio. This values also shows where x is, when y = 0.5.
        /// </summary>
        public double EffectiveOffsetRatio => 0.25;

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void GenerateCache()
        {
            Values = new float[Length];
            IntegralValues = new float[Length];
            for (int i = 0; i < Length; i++)
            {
                Values[i] = (float)(Math.Cos(i * 2 * Math.PI / Length - Math.PI) * 0.5 + 0.5);
                IntegralValues[i] = (float)((i / 2.0 - Length * Math.Sin(i * 2.0 * Math.PI / Length) / (4.0 * Math.PI)) / (Length));
            }
            CacheValueList.Clear();
            CacheValueList.Add(Values);
            CacheValueList.Add(IntegralValues);
        }

        /// <inheritdoc/>
        protected override void OnCacheLoaded()
        {
            Values = CacheValueList[0];
            IntegralValues = CacheValueList[1];
        }

        #region Apply

        /// <summary>
        /// Applies this Hann window filter on the given float array.
        /// </summary>
        /// <param name="data">The float array data.</param>
        /// <param name="windowType">The type of the windowing to apply.</param>
        /// <returns>The float array containing the filtered data.</returns>
        public float[] Apply(float[] data, WindowingType windowType) => Apply(data, 0, data.Length, windowType);

        /// <summary>
        /// Applies this Hann window filter on the given float array.
        /// </summary>
        /// <param name="data">The float array data.</param>
        /// <param name="startIndex">The start index of the relevant part of the data.</param>
        /// <param name="endIndex">The end index of the relevant part of the data.</param>
        /// <param name="windowType">The type of the windowing to apply.</param>
        /// <returns>The float array containing the filtered data.</returns>
        public float[] Apply(float[] data, int startIndex, int endIndex, WindowingType windowType)
        {
            return windowType switch
            {
                WindowingType.Full => ApplyFull(data, startIndex, endIndex),
                WindowingType.HalfAscending => ApplyHalfAscending(data, startIndex, endIndex),
                WindowingType.HalfDescending => ApplyHalfDescending(data, startIndex, endIndex),
                _ => throw new InvalidEnumValueException(windowType)
            };
        }

        /// <summary>
        /// Applies the full Hann window filter on the given float array.
        /// </summary>
        /// <param name="data">The float array data.</param>
        /// <param name="startIndex">The start index of the relevant part of the data.</param>
        /// <param name="endIndex">The end index of the relevant part of the data.</param>
        /// <returns>The float array containing the filtered data.</returns>
        public float[] ApplyFull(float[] data, int startIndex, int endIndex)
        {
            float[] result = new float[endIndex - startIndex];
            for (int i = 0; i < result.Length; i++)
            {
                int hannIndex = (int)(((double)i / result.Length) * Length);
                result[i] = data[startIndex + i] * Values[hannIndex];
            }
            return result;
        }

        /// <summary>
        /// Applies the first half of this Hann window filter on the given float array.
        /// </summary>
        /// <param name="data">The float array data.</param>
        /// <param name="startIndex">The start index of the relevant part of the data.</param>
        /// <param name="endIndex">The end index of the relevant part of the data.</param>
        /// <returns>The float array containing the filtered data.</returns>
        public float[] ApplyHalfAscending(float[] data, int startIndex, int endIndex)
        {
            float[] result = new float[endIndex - startIndex];
            for (int i = 0; i < result.Length; i++)
            {
                int hannIndex = (int)(((double)i / result.Length) * (Length / 2));
                result[i] = data[startIndex + i] * Values[hannIndex];
            }
            return result;
        }

        /// <summary>
        /// Applies the second half of this Hann window filter on the given float array.
        /// </summary>
        /// <param name="data">The float array data.</param>
        /// <param name="startIndex">The start index of the relevant part of the data.</param>
        /// <param name="endIndex">The end index of the relevant part of the data.</param>
        /// <returns>The float array containing the filtered data.</returns>
        public float[] ApplyHalfDescending(float[] data, int startIndex, int endIndex)
        {
            float[] result = new float[endIndex - startIndex];
            for (int i = 0; i < result.Length; i++)
            {
                int hannIndex = (int)(((double)i / result.Length) * (Length / 2)) + (Length / 2);
                result[i] = data[startIndex + i] * Values[hannIndex];
            }
            return result;
        }


        #endregion

        #region Integral

        /// <summary>
        /// Gets the normalized area of this Hann window between a given range.
        /// </summary>
        /// <param name="windowType">The type of the windowing.</param>
        /// <returns>The float normalize area of this Hann window between the given range.</returns>
        public float GetIntegralOf(WindowingType windowType)
        {
            return windowType switch
            {
                WindowingType.Full => GetIntegralOf(0, 1),
                WindowingType.HalfAscending => GetIntegralOf(0, 0.5),
                WindowingType.HalfDescending => GetIntegralOf(0.5, 1),
                _ => throw new InvalidEnumValueException(windowType)
            };
        }

        /// <summary>
        /// Gets the normalized area of this Hann window depending on the given <see cref="WindowingType"/>.
        /// </summary>
        /// <param name="ratioStart">The start ratio of the range.</param>
        /// <param name="ratioEnd">The end ratio of the range.</param>
        /// <returns>The float normalize area of this Hann window between the given range.</returns>
        public float GetIntegralOf(double ratioStart, double ratioEnd)
        {
            return IntegralValues[(int)(ratioEnd * (Length - 1))] - IntegralValues[(int)(ratioStart * (Length - 1))];
        }

        #endregion

        #region Area Ratio

        /// <summary>
        /// Gets the area ratio of the Hann window depending on the given <see cref="WindowingType"/>.
        /// </summary>
        /// <param name="ratioStart">The start ratio of the range.</param>
        /// <param name="ratioEnd">The end ratio of the range.</param>
        /// <returns>The area ratio of this Hann window depending on the given <see cref="WindowingType"/>.</returns>
        public float GetAreaRatioOf(double ratioStart, double ratioEnd)
        {
            return GetIntegralOf(ratioStart, ratioEnd) / (float)(ratioEnd - ratioStart);
        }

        /// <summary>
        /// Gets the area ratio of the Hann window between a given range.
        /// </summary>
        /// <param name="windowType">The type of the windowing.</param>
        /// <returns>The area ratio of this Hann window by the given.</returns>
        public float GetAreaRatioOf(WindowingType windowType)
        {
            return windowType switch
            {
                WindowingType.Full => GetAreaRatioOf(0, 1),
                WindowingType.HalfAscending => GetAreaRatioOf(0, 0.5),
                WindowingType.HalfDescending => GetAreaRatioOf(0.5, 1),
                _ => throw new InvalidEnumValueException(windowType)
            };
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HannWindowCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        /// <param name="length">The length of the cached array.</param>
        public HannWindowCache(string cacheLocation, int length) : base(cacheLocation)
        {
            Length = length;
        }

        #endregion
    }
}
