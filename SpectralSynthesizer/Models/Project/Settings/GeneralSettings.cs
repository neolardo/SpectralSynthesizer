using Newtonsoft.Json;
using System;
using System.IO;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Contains all the application related settings which does not differ among projects.
    /// </summary>
    public class GeneralSettings : BaseModel
    {
        #region Properties

        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing ogg events when the <see cref="Theme"/> property changes.
        /// </summary>
        public delegate void ThemeChangedDelegate();

        /// <summary>
        /// Fires off when the <see cref="Theme"/> property has changed.
        /// </summary>
        public event ThemeChangedDelegate ThemeChanged;

        #endregion

        private ApplicationTheme _theme;

        /// <summary>
        /// The theme of the application.
        /// </summary>
        public ApplicationTheme Theme
        {
            get { return _theme; }
            set
            {
                _theme = value;
                ThemeChanged?.Invoke();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the general settings from a .json file.
        /// </summary>
        /// <param name="filePath">The full path to the .json file.</param>
        private void Load(string filePath)
        {
            if (File.Exists(filePath) == false)
            {
                Theme = ApplicationTheme.Misty;
                Save(filePath);
            }
            else
            {
                string textinput = File.ReadAllText(filePath);
                var save = JsonConvert.DeserializeObject<GeneralSettings>(textinput, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TypeNameHandling = TypeNameHandling.Auto
                });
                Theme = save.Theme;
            }
        }

        /// <summary>
        /// Method to save the settings to the given .json file.
        /// </summary>
        public void Save(string filePath)
        {
            string textOutput = JsonConvert.SerializeObject(
                this,
                Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TypeNameHandling = TypeNameHandling.Auto
                });
            File.WriteAllText(filePath, textOutput);
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettings"/> class.
        /// </summary>
        /// <param name="theme">The theme of the application.</param>
        public GeneralSettings(ApplicationTheme theme)
        {
            Theme = theme;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettings"/> class from a given .json file.
        /// </summary>
        /// <param name="filePath">The fullpath to the .json file.</param>
        public GeneralSettings(string filePath)
        {
            Load(filePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettings"/> class.
        /// </summary>
        [JsonConstructor]
        public GeneralSettings() { }

        #endregion

    }
}
