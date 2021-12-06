using BrendanGrant.Helpers.FileAssociation;
using NAudio.Wave;
using Newtonsoft.Json;
using SpectralSynthesizer.Models.Audio.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Contains every information of a project.
    /// </summary>
    public class ProjectModel : BaseModel
    {

        #region Properties

        #region Static

        #region Audio

        /// <summary>
        /// The global minimum decibel amplitude used in the application.
        /// </summary>
        public static float GlobalMinimumDecibel => -110;

        /// <summary>
        /// The relative minimum decibel amplitude used in the application.
        /// </summary>
        public static float RelativeMinimumDecibel => -70;

        /// <summary>
        /// The decibel difference between the loudest single sound and the loudest overall sound. 
        /// </summary>
        public static float MaximumSingleDecibelAmplitudeDelta => 12;

        /// <summary>
        /// The lowest hearable frequency.
        /// </summary>
        public static double LowestFrequency => 16.3516;

        /// <summary>
        /// The highest hearable frequency.
        /// </summary>
        public static double HighestFrequency => 20000.0;

        /// <summary>
        /// The number of subnotes in each note. It is only used for visualization.
        /// </summary>
        public static int TonePerNote => 8;

        /// <summary>
        /// The total number of notes in the hearable spectrum.
        /// </summary>
        public static int TotalNoteNumber => (int)(Math.Log10(HighestFrequency / LowestFrequency) / Math.Log10(2) * 12);

        /// <summary>
        /// The total number of discrete frequencies in the hearable spectrum.
        /// It is based on the <see cref="TonePerNote"/> value.
        /// </summary>
        public static int TotalDiscreteFrequencyNumber => TotalNoteNumber * TonePerNote;

        /// <summary>
        /// The standard waveformat of any audio file this application uses.
        /// </summary>
        public static WaveFormat StandardWaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

        /// <summary>
        /// The samplerate of the <see cref="StandardWaveFormat"/>.
        /// </summary>
        public static int StandardSampleRate => StandardWaveFormat.SampleRate * StandardWaveFormat.Channels;

        /// <summary>
        /// The minimum sampling length in <see cref="StandardSampleRate"/> float count.
        /// </summary>
        public static float MaximumSamplingFrequency => 1000f;

        /// <summary>
        /// The number of samples to fade in and out.
        /// </summary>
        public const int FadeSampleNumber = 200;

        #endregion

        #region File Extensions

        /// <summary>
        /// The project file extension.
        /// </summary>
        public const string ProjectFileExtension = "sfp";

        /// <summary>
        /// The <see cref="Instrument"/> bundle file extension.
        /// </summary>
        public const string InstrumentBundleFileExtension = "sfi";

        #endregion

        #region Locations

        /// <summary>
        /// The location to the application's folder.
        /// </summary>
        public static string ApplicationFolderLocation => Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpectralSynthesizer");

        /// <summary>
        /// The location of the cached decibel values.
        /// </summary>
        public static string DecibelCacheLocation => Path.Combine(ApplicationFolderLocation, "decibels.json"); 

        /// <summary>
        /// The location of the cached discrete frequency values.
        /// </summary>
        public static string FrequencyCacheLocation => Path.Combine(ApplicationFolderLocation, "frequencies.json");

        /// <summary>
        /// The location of the cached logarithm values.
        /// </summary>
        public static string LogarithmCacheLocation => Path.Combine(ApplicationFolderLocation,  "logarithms.json"); 

        /// <summary>
        /// The location of the cached sine wave values.
        /// </summary>
        public static string SineWaveCacheLocation => Path.Combine(ApplicationFolderLocation, "sines.json"); 

        /// <summary>
        /// The location of the cached Hann window values.
        /// </summary>
        public static string HannWindowCacheLocation => Path.Combine(ApplicationFolderLocation,  "hann.json");

        /// <summary>
        /// The location of the temporary midi wave .wav file.
        /// </summary>
        public static string TempMidiWaveLocation => Path.Combine(ApplicationFolderLocation,  "tempmidi.wav");
        /// <summary>
        /// The location of the temporary rendered wave .wav file.
        /// </summary>
        public static string TempRenderedWaveLocation => Path.Combine(ApplicationFolderLocation,  "temprender.wav"); 

        /// <summary>
        /// The location of the general settings .json file.
        /// </summary>
        private static string GeneralSettingsLocation => Path.Combine(ApplicationFolderLocation, "settings.json");

        #endregion

        /// <summary>
        /// The default name of the project.
        /// </summary>
        private static string DefaultProjectName => "untitled";

        /// <summary>
        /// The maximum number of characters a name string can contain.
        /// </summary>
        public static int MaximumNameLength => 40;

        /// <summary>
        /// The starting page of the application.
        /// </summary>
        private static ApplicationPage StartingPage => ApplicationPage.InstrumentBuilder;

        #endregion

        #region Locations

        [JsonProperty]
        /// <summary>
        /// The location of this project.
        /// </summary>
        public string ProjectLocation { get; private set; } = "";

        #endregion

        #region Project Settings

        /// <summary>
        /// The name of this project.
        /// </summary>
        public string Name { get; set; } = DefaultProjectName;

        #endregion

        #region General Settings

        [JsonProperty]
        /// <summary>
        /// The general settings of the application.
        /// </summary>
        public GeneralSettings GeneralSettings { get; private set; }

        #endregion

        #region Pages

        [JsonProperty]
        /// <summary>
        /// The currently visible page of the project.
        /// </summary>
        public ApplicationPage CurrentPage { get; private set; } = StartingPage;

        [JsonProperty]
        /// <summary>
        /// The model of the <see cref="InstrumentBuilderControl"/>.
        /// </summary>
        public InstrumentBuilderModel InstrumentBuilder { get; private set; } = new InstrumentBuilderModel();

        [JsonProperty]
        /// <summary>
        /// The model of the <see cref="AudioRendererControl"/>.
        /// </summary>
        public AudioRendererModel AudioRenderer { get; private set; } = new AudioRendererModel();

        #endregion

        #region Audio Data

        [JsonProperty]
        /// <summary>
        /// All the <see cref="Instrument"/>s this project contains.
        /// </summary>
        public ObservableCollection<Instrument> Instruments { get; } = new ObservableCollection<Instrument>();

        #endregion

        #endregion

        #region Methods

        #region Save

        /// <summary>
        /// Saves this project.
        /// </summary>
        public void SaveProject()
        {
            if (string.IsNullOrEmpty(ProjectLocation) || string.IsNullOrWhiteSpace(ProjectLocation))
            {
                var saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = $"SpectralSynthesizer project files (*.{ProjectFileExtension})|*.{ProjectFileExtension}";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProjectLocation = saveFileDialog.FileName;
                }
                else
                    return;
            }
            string textOutput = JsonConvert.SerializeObject(
                this,
                Formatting.Indented, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.Auto
                });
            File.WriteAllText(ProjectLocation, textOutput);
        }

        /// <summary>
        /// Saves the project to the given location.
        /// </summary>
        /// <param name="location">The location to save this project.</param>
        public void SaveProject(string location)
        {
            ProjectLocation = location;
            SaveProject();
        }

        /// <summary>
        /// Saves the general settings of this application to the given .json file.
        /// </summary>
        public void SaveGeneralSettings()
        {
            GeneralSettings.Save(GeneralSettingsLocation);
        }

        #endregion

        #region Import

        /// <summary>
        /// Imports a file from it's fullpath.
        /// </summary>
        /// <param name="filePath">The fullpath to the file.</param>
        public void Import(string filePath)
        {
            if (filePath.Length < 4)
                return;
            string extension = filePath.Substring(filePath.Length - 3);
            switch (extension)
            {
                case ProjectFileExtension:
                    ImportProject(filePath);
                    break;
                case InstrumentBundleFileExtension:
                    ImportInstruments(filePath);
                    break;
            }
        }

        /// <summary>
        /// Imports a project from filepath.
        /// </summary>
        /// <param name="fullPath">The full path to the file.</param>
        private void ImportProject(string fullPath)
        {
            string textinput = File.ReadAllText(fullPath);
            var project = JsonConvert.DeserializeObject<ProjectModel>(textinput, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto,
            });
            CreateBlankProject();
            Name = project.Name;
            CurrentPage = project.CurrentPage;
            ProjectLocation = project.ProjectLocation;
            InstrumentBuilder = project.InstrumentBuilder;
            AudioRenderer = project.AudioRenderer;
            foreach (var i in project.Instruments)
            {
                Instruments.Add(i);
            }
            if (InstrumentBuilder.Instrument != null)
            {
                InstrumentBuilder.Instrument = Instruments.FirstOrDefault(_ => _.Name == InstrumentBuilder.Instrument.Name);
            }
            if (AudioRenderer.Instrument != null)
            {
                AudioRenderer.Instrument = Instruments.FirstOrDefault(_ => _.Name == AudioRenderer.Instrument.Name);
            }
            IoC.Get<InstrumentBuilderViewModel>().LoadModel(InstrumentBuilder);
            IoC.Get<AudioRendererViewModel>().LoadModel(AudioRenderer);
            if (Instruments.Count > 0)
            {
                if (InstrumentBuilder.Instrument == null)
                {
                    IoC.Get<InstrumentBuilderViewModel>().InstrumentsComboBox.SelectElement(Instruments[0]);
                }
                if (AudioRenderer.Instrument == null)
                {
                    IoC.Get<AudioRendererViewModel>().InstrumentsComboBox.SelectElement(Instruments[0]);
                }
            }
        }

        /// <summary>
        /// Imports a list of <see cref="Instrument"/>s to the application.
        /// </summary>
        /// <param name="fullPath">The full path to the file.</param>
        private void ImportInstruments(string fullPath)
        {
            string textinput = File.ReadAllText(fullPath);
            var bundle = JsonConvert.DeserializeObject<List<Instrument>>(textinput, new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto
            });
            foreach (var i in bundle)
            {
                i.TrySetName(CalculateAvailableInstrumentName(i.Name));
                Instruments.Add(i);
            }
            if (Instruments.Count > 0)
            {
                if (InstrumentBuilder.Instrument == null)
                {
                    IoC.Get<InstrumentBuilderViewModel>().InstrumentsComboBox.SelectElement(Instruments[0]);
                }
                if (AudioRenderer.Instrument == null)
                {
                    IoC.Get<AudioRendererViewModel>().InstrumentsComboBox.SelectElement(Instruments[0]);
                }
            }
        }

        /// <summary>
        /// Calculates the next possible <see cref="Instrument.Name"/> which does not exists among the <see cref="Instruments"/> list yet from the given starting name.
        /// </summary>
        /// <returns>The next possible <see cref="Instrument.Name"/> which does not exists among the <see cref="Instruments"/> list yet.</returns>
        public string CalculateAvailableInstrumentName(string startingName)
        {
            int number = 1;
            string name = $"{startingName + number.ToString()}";
            var names = (from i in Instruments
                         select i.Name).ToList();
            if (!names.Contains(startingName))
            {
                return startingName;
            }
            while (names.Contains(name))
            {
                number++;
                name = $"{startingName + number.ToString()}";
            }
            return name;
        }

        #endregion

        #region Export



        #endregion

        /// <summary>
        /// Navigates to a given page.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        public void NavigateTo(ApplicationPage page)
        {
            CurrentPage = page;
        }

        /// <summary>
        /// Creates a blank project.
        /// </summary>
        public void CreateBlankProject()
        {
            StopEverything();
            Name = DefaultProjectName;
            ProjectLocation = "";
            InstrumentBuilder = new InstrumentBuilderModel();
            AudioRenderer = new AudioRendererModel();
            Instruments.Clear();
            IoC.Get<InstrumentBuilderViewModel>().LoadModel(InstrumentBuilder);
            IoC.Get<AudioRendererViewModel>().LoadModel(AudioRenderer);
        }

        /// <summary>
        /// Stops all the currently ongoing processes.
        /// </summary>
        private void StopEverything()
        {
            IoC.Get<AudioPlayer>().RequestStop();
            Computer.CancelEverything();
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new NotImplementedException();
        }

        #region Application Initializers

        /// <summary>
        /// Creates the appdata folder if it does not exists yet.
        /// </summary>
        public void CreateAppDataFolder()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpectralSynthesizer");
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Creates all the file extentions this application uses.
        /// </summary>
        public void CreateFileExtensions()
        {
            var exepath = System.Reflection.Assembly.GetEntryAssembly().Location;
            FileAssociationInfo sfi = new FileAssociationInfo(".sfi");
            if (!sfi.Exists)
            {
                sfi.Create("SpectralSynthesizer");
                sfi.OpenWithList = new string[]
               { "notepad.exe","spectralsynthesizer.exe" };
            }
            ProgramAssociationInfo sfipai = new ProgramAssociationInfo(sfi.ProgID);
            if (!sfipai.Exists)
            {
                sfipai.Create
                (
                "SpectralSynthesizer's file type storing instruments.",
                new ProgramVerb
                     (
                     "Open",
                     exepath
                     )
                   );
            }
            FileAssociationInfo sfp = new FileAssociationInfo(".sfp");
            if (!sfp.Exists)
            {
                sfp.Create("SpectralSynthesizer");
                sfp.OpenWithList = new string[]
               { "notepad.exe","spectralsynthesizer.exe" };
            }
            ProgramAssociationInfo sfppai = new ProgramAssociationInfo(sfp.ProgID);
            if (!sfppai.Exists)
            {
                sfppai.Create
                (
                "SpectralSynthesizer project file type.",
                new ProgramVerb
                     (
                     "Open",
                     exepath
                     )
                   );
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProjectModel()
        {
            CreateAppDataFolder();
            GeneralSettings = new GeneralSettings(GeneralSettingsLocation);
            // Continue the property changed chain
            GeneralSettings.PropertyChanged += OnPropertyChanged;
            InstrumentBuilder.PropertyChanged += OnPropertyChanged;
            AudioRenderer.PropertyChanged += OnPropertyChanged;
        }

        #endregion
    }
}
