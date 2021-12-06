using System;
using System.Collections.Generic;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A cache storing logarithmic values.
    /// </summary>
    public class LogarithmCache : NumericCache
    {
        #region Properties

        /// <summary>
        /// The cached input values between the start and end values.
        /// </summary>
        private List<float> Values { get; set; }

        /// <summary>
        /// The cached output values between the <see cref="StartLogValue"/> and the <see cref="EndLogValue"/>.
        /// </summary>
        private float[] LogValues { get; set; }

        /// <summary>
        /// The first logarithmic value in the cache.
        /// </summary>
        private float StartLogValue { get; }

        /// <summary>
        /// The last logarithmic value in the cache.
        /// </summary>
        private float EndLogValue { get; }

        /// <summary>
        /// The number of steps between two whole logarithmic values.</param>
        /// </summary>
        private int Resolution { get; }

        /// <summary>
        /// The difference between two cached logarithmical values.</param>
        /// </summary>
        private float LogStep { get; }

        /// <summary>
        /// The base of the logarithm.
        /// </summary>
        private float Base { get; }

        /// <summary>
        /// The log2 length of the cache.
        /// </summary>
        private int LogLength { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void GenerateCache()
        {
            int length = (int)(EndLogValue - StartLogValue) * Resolution;
            Values = new List<float>();
            LogValues = new float[length];
            for (int i = 0; i < length; i++)
            {
                LogValues[i] = StartLogValue + i * LogStep;
                Values.Add(MathF.Pow(Base, LogValues[i]));
            }
            CacheValueList.Clear();
            CacheValueList.Add(Values.ToArray());
            CacheValueList.Add(LogValues);
        }

        /// <inheritdoc/>
        protected override void OnCacheLoaded()
        {
            Values = new List<float>(CacheValueList[0]);
            LogValues = CacheValueList[1];
        }

        #region Get Data

        /// <summary>
        /// Gets the logarithm of a given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The logarithm of the given value.</returns>
        public float GetLogarithmOf(float value)
        {
            int index = Values.AscendingFloatBinarySearch(value, LogLength);
            return LogValues[index];
        }


        /// <summary>
        /// Gets the value of a given value's logarithm.
        /// </summary>
        /// <param name="logValue">The given value's logarithm.</param>
        /// <returns>The value.</returns>
        public float GetValueOf(float logValue)
        {
            int index = Math.Clamp((int)((logValue - StartLogValue) / LogStep), 0, Values.Count - 1);
            return Values[index];
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogarithmCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        /// <param name="logBase">The logarithm's base.</param>
        /// <param name="start">The start value of the cache. Should not be smaller than 1.</param>
        /// <param name="end">The end value of the cache.</param>
        /// <param name="resolution">The resolution of the cache between two whole logarithmical values.</param>
        public LogarithmCache(string cacheLocation, float logBase, float start, float end, int resolution) : base(cacheLocation)
        {
            Base = logBase;
            StartLogValue = MathF.Floor(MathF.Log(start, Base));
            EndLogValue = MathF.Ceiling(MathF.Log(end, Base));
            Resolution = resolution;
            LogStep = 1f / Resolution;
            LogLength = (int)Math.Ceiling(Math.Log2((int)(EndLogValue - StartLogValue) * Resolution));
        }

        #endregion
    }
}
