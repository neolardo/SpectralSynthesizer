using System;
using System.Collections.Generic;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A cache storing decibel values.
    /// </summary>
    public class DecibelCache : NumericCache
    {
        /// <summary>
        /// The cached PCM input values between the PCM value of the <see cref="LowestDecibel"/> and 1.
        /// </summary>
        private List<float> PCMValues { get; set; }

        /// <summary>
        /// The cached decibel output values between the <see cref="LowestDecibel"/> and 0 db.
        /// </summary>
        private float[] DecibelValues { get; set; }

        /// <summary>
        /// The lowest Decibel value.
        /// </summary>
        private float LowestDecibel { get; }

        /// <summary>
        /// The number of steps between two whole decibel values.</param>
        /// </summary>
        private int Resolution { get; }

        /// <summary>
        /// The difference between two cached decibel values.</param>
        /// </summary>
        private float DecibelStep { get; }

        /// <summary>
        /// The log2 length of the cache.
        /// </summary>
        private int LogLength { get; }

        #region Methods

        /// <inheritdoc/>
        protected override void GenerateCache()
        {
            int length = (int)(-1 * LowestDecibel) * Resolution;
            PCMValues = new List<float>();
            DecibelValues = new float[length];
            for (int i = 0; i < length; i++)
            {
                DecibelValues[i] = -1f * i * DecibelStep;
                PCMValues.Add((float)Math.Pow(10, (DecibelValues[i] / 20.0)));
            }
            CacheValueList.Clear();
            CacheValueList.Add(PCMValues.ToArray());
            CacheValueList.Add(DecibelValues);
        }

        /// <inheritdoc/>
        protected override void OnCacheLoaded()
        {
            PCMValues = new List<float>(CacheValueList[0]);
            DecibelValues = CacheValueList[1];
        }

        #region Get Data

        /// <summary>
        /// Gets the decibel of a given PCM value.
        /// </summary>
        /// <param name="pcmValue">The pcm value.</param>
        /// <returns>The decibel of the given PCM value.</returns>
        public float GetDecibelOf(float pcmValue)
        {
            int index = PCMValues.DescendingFloatBinarySearch(pcmValue, LogLength);
            return DecibelValues[index];
        }

        /// <summary>
        /// Gets the PCM value of a given decibel value.
        /// </summary>
        /// <param name="decibelValue">The decibel value.</param>
        /// <returns>The PCM value of the given decibel value.</returns>
        public float GetPCMOf(float decibelValue)
        {
            int index = Math.Clamp(-1 * (int)(decibelValue / DecibelStep), 0, PCMValues.Count - 1);
            return PCMValues[index];
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DecibelCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        /// <param name="lowestDecibel">The lowest decibel value of the cache. Should be a negative value.</param>
        /// <param name="resolution">The resolution of the cache between two whole decibel values.</param>
        public DecibelCache(string cacheLocation, float lowestDecibel, int resolution) : base(cacheLocation)
        {
            LowestDecibel = lowestDecibel;
            Resolution = resolution;
            DecibelStep = 1f / Resolution;
            LogLength = (int)Math.Ceiling(Math.Log2(-1 * LowestDecibel * Resolution));
        }

        #endregion
    }
}
