using SpectralSynthesizer.Models;
using System;
using System.Collections.Generic;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A cache storing discrete and continous frequency values.
    /// </summary>
    public class DiscreteFrequencyCache : NumericCache
    {
        #region Properties

        /// <summary>
        /// The cached frequency values.
        /// </summary>
        public List<float> Values { get; private set; }

        /// <summary>
        /// The length of the cached array.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// The log2 length of the <see cref="Length"/> property used in binary search.
        /// </summary>
        private int LogLength { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void GenerateCache()
        {
            Values = new List<float>();
            for (int i = 0; i < Length; i++)
            {
                Values.Add((float)(ProjectModel.LowestFrequency * Math.Pow(2.0, i * 1.0 / (12.0 * ProjectModel.TonePerNote))));
            }
            if (Values[Length - 1] > ProjectModel.HighestFrequency)
            {
                Values[Length - 1] = (float)ProjectModel.HighestFrequency;
            }
            CacheValueList.Clear();
            CacheValueList.Add(Values.ToArray());
        }

        /// <inheritdoc/>
        protected override void OnCacheLoaded()
        {
            Values = new List<float>(CacheValueList[0]);
        }

        #region Get Data

        /// <summary>
        /// Gets the discrete frequency from the given frequency value.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <returns>The discrete frequency.</returns>
        public int GetDiscreteFrequency(float frequency)
        {
            return Values.AscendingFloatBinarySearch(frequency, LogLength);
        }

        /// <summary>
        /// Gets the frequency value from the given discrete frequency.
        /// </summary>
        /// <param name="discreteFrequency">The discrete frequency.</param>
        /// <returns>The frequency.</returns>
        public float GetFrequency(int discreteFrequency) => Values[Math.Clamp(discreteFrequency, 0, Length - 1)];

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscreteFrequencyCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        public DiscreteFrequencyCache(string cacheLocation) : base(cacheLocation)
        {
            Length = ProjectModel.TotalDiscreteFrequencyNumber;
            LogLength = (int)Math.Ceiling(Math.Log2(Length));
        }

        #endregion
    }
}
