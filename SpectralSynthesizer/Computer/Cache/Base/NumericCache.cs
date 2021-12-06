using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SpectralSynthesizer
{
    /// <summary>
    /// An abstract class for storing and using numeric cache values to boost performance.
    /// </summary>
    public abstract class NumericCache
    {
        #region Properties

        /// <summary>
        /// The location of the cached file which stores this cache's data.
        /// </summary>
        public string CacheLocation { get; protected set; }

        /// <summary>
        /// The list containing the cached numeric value array(s).
        /// </summary>
        public List<float[]> CacheValueList { get; protected set; } = new List<float[]>();

        #endregion

        #region Methods

        /// <summary>
        /// Generates the cached values.
        /// </summary>
        protected abstract void GenerateCache();

        /// <summary>
        /// Called after the cache has been loaded.
        /// </summary>
        protected abstract void OnCacheLoaded();

        /// <summary>
        /// Saves the generated cache values to the <see cref="CacheLocation"/>.
        /// <see cref="GenerateValues"/> should be called before this.
        /// </summary>
        private void SaveCache()
        {
            string textoutput = JsonConvert.SerializeObject(
                CacheValueList,
                Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    TypeNameHandling = TypeNameHandling.Auto
                });
            File.WriteAllText(CacheLocation, textoutput);
        }

        /// <summary>
        /// Loads the generated cache values from the <see cref="CacheLocation"/> if it exists, generates and saves them otherwise.
        /// </summary>
        public void LoadCache()
        {
            if (File.Exists(CacheLocation))
            {
                string textinput = File.ReadAllText(CacheLocation);
                var save = JsonConvert.DeserializeObject<List<float[]>>(textinput, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    TypeNameHandling = TypeNameHandling.Auto
                });
                CacheValueList = save;
                OnCacheLoaded();
            }
            else
            {
                GenerateCache();
                SaveCache();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Abstract constructor for the <see cref="NumericCache"/> class.
        /// </summary>
        /// <param name="cacheLocation">The location of the cached values.</param>
        public NumericCache(string cacheLocation)
        {
            CacheLocation = cacheLocation;
        }

        #endregion
    }
}
