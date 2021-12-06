using System;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A cache storing sine wave values.
    /// </summary>
    public class SineWaveCache : NumericCache
    {
        #region Properties

        /// <summary>
        /// The cached sine wave values.
        /// </summary>
        public float[] Values { get; private set; }

        /// <summary>
        /// The length of the cached array.
        /// </summary>
        public int Length { get; private set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void GenerateCache()
        {
            Values = new float[Length];
            for (int i = 0; i < Length; i++)
            {
                Values[i] = (float)Math.Sin(2.0 * Math.PI * (double)i / Length);
            }
            CacheValueList.Clear();
            CacheValueList.Add(Values);
        }

        /// <inheritdoc/>
        protected override void OnCacheLoaded()
        {
            Values = CacheValueList[0];
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SineWaveCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        /// <param name="length">The length of the cached array.</param>
        public SineWaveCache(string cacheLocation, int length) : base(cacheLocation)
        {
            Length = length;
        }

        #endregion
    }
}
