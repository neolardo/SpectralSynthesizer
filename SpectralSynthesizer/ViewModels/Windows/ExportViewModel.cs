using NAudio.Wave;
using Newtonsoft.Json;
using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The datacontext of the export window
    /// </summary>
    public class ExportViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The export window.
        /// </summary>
        private ExportWindow Window { get; set; }

        /// <summary>
        /// The height of the title of this window.
        /// </summary>
        public int TitleHeight { get; set; } = 28;

        /// <summary>
        /// The currently selected eyportation type.
        /// </summary>
        public ExportationType ExportationType { get; set; } = ExportationType.Project;

        /// <summary>
        /// The list of available <see cref="Instrument"/>s.
        /// </summary>
        public SelectionBoxViewModel<Instrument> InstrumentsSelectionBox { get; } = new SelectionBoxViewModel<Instrument>(IoC.Get<ProjectModel>().Instruments);

        /// <summary>
        /// The rendered <see cref="Wave"/> result.
        /// </summary>
        private Wave Wave { get; set; }

        /// <summary>
        /// The full path to expot the given type of data, including the file extension.
        /// </summary>
        private string ExportPath { get; set; }

        /// <summary>
        /// Indicates the current <see cref="ExportationState"/>.
        /// </summary>
        public ExportationState ExportState { get; set; } = ExportationState.Init;

        /// <summary>
        /// Indicates whether the exportation is enabled or not.
        /// </summary>
        public bool IsExportEnabled
        {
            get
            {
                switch (ExportationType)
                {
                    case ExportationType.Project:
                        return true;
                    case ExportationType.Instrument:
                        return InstrumentsSelectionBox.GetSelectedModels().Count > 0;
                    case ExportationType.Wave:
                        return Wave != null;
                    default:
                        throw new InvalidEnumValueException(ExportationType);
                }
            }
        }

        /// <summary>
        /// True if only the selected part should be exported of the rendered <see cref="Wave"/>.
        /// </summary>
        public bool WaveSelectedPartOnly { get; set; } = false;

        #endregion

        #region Commands

        /// <summary>
        /// Command for closing this window.
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// Command for setting the export location.
        /// </summary>
        public ICommand BrowseCommand { get; set; }

        /// <summary>
        /// Command for selecting an <see cref="SpectralSynthesizer.ExportationType"/>
        /// </summary>
        public ICommand SelectExportationTypeCommand { get; set; }

        /// <summary>
        /// Command to select all elements in the <see cref="InstrumentsSelectionBox"/> <see cref="SelectionBox"/>.
        /// </summary>
        public ICommand SelectAllInstrumentsCommand { get; set; }

        /// <summary>
        /// Command to deselect all elements in the <see cref="InstrumentsSelectionBox"/> <see cref="SelectionBox"/>.
        /// </summary>
        public ICommand DeselectAllInstrumentsCommand { get; set; }

        #endregion

        #region Methods

        #region Export

        /// <summary>
        /// Export the selected type of file.
        /// </summary>
        private void Export()
        {
            switch (ExportationType)
            {
                case ExportationType.Project:
                    ExportProject();
                    break;
                case ExportationType.Instrument:
                    ExportInstruments();
                    break;
                case ExportationType.Wave:
                    ExportWave();
                    break;
                default:
                    throw new InvalidEnumValueException(ExportationType);
            }
        }

        /// <summary>
        /// Exports this <see cref="ProjectModel"/>.
        /// </summary>
        private void ExportProject()
        {
            IoC.Get<ProjectModel>().SaveProject(ExportPath);
        }


        /// <summary>
        /// Exports the selected list of <see cref="Instrument"/>s.
        /// </summary>
        private void ExportInstruments()
        {
            var bundle = InstrumentsSelectionBox.GetSelectedModels();
            string textOutput = JsonConvert.SerializeObject(
                bundle, new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TypeNameHandling = TypeNameHandling.Auto
                });
            File.WriteAllText(ExportPath, textOutput);
        }

        /// <summary>
        /// Exports the rendered <see cref="Models.Audio.Data.Wave"/>of the <see cref="AudioRendererModel"/>.
        /// </summary>
        private void ExportWave()
        {
            WaveFileWriter writer = new WaveFileWriter(ExportPath, Wave.WaveFormat);
            float[] floatData = WaveSelectedPartOnly ? Wave.GetSelectedData(IoC.Get<AudioRendererViewModel>().WaveViewViewModel.Selection) : Wave.Data;
            int byteLength = floatData.Length * 4;
            byte[] byteData = new byte[byteLength];
            Buffer.BlockCopy(floatData, 0, byteData, 0, byteLength);
            writer.Write(byteData, 0, byteLength);
            writer.Flush();
            writer.Dispose();
        }

        #endregion

        /// <summary>
        /// Opens a save file dialog to set the exportation location.
        /// </summary>
        private void Browse()
        {
            if (IsExportEnabled)
            {
                ExportState = ExportationState.InProgress;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                string filtertype = "";
                switch (ExportationType)
                {
                    case ExportationType.Project:
                        filtertype = $"SpectralSynthesizer project files (*.{ProjectModel.ProjectFileExtension})|*.{ProjectModel.ProjectFileExtension}";
                        break;
                    case ExportationType.Instrument:
                        filtertype = $"SpectralSynthesizer instrument files (*.{ProjectModel.InstrumentBundleFileExtension})|*.{ProjectModel.InstrumentBundleFileExtension}";
                        break;
                    case ExportationType.Wave:
                        filtertype = "Wave files (*.wav)|*.wav";
                        break;
                }
                saveFileDialog.Filter = filtertype;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ExportPath = saveFileDialog.FileName;
                    try
                    {
                        Export();
                    }
                    catch (Exception)
                    {
                        ExportState = ExportationState.Fail;
                        return;
                    }
                    ExportState = ExportationState.Success;
                }
                else
                {
                    ExportState = ExportationState.Init;
                }
            }
        }

        /// <summary>
        /// Selects the given <see cref="SpectralSynthesizer.ExportationType"/>;
        /// </summary>
        /// <param name="expotype">The new exportation type</param>
        private void SelectExportationType(object expotype)
        {
            switch ((string)expotype)
            {
                case "project":
                    ExportationType = ExportationType.Project;
                    break;
                case "instrument":
                    ExportationType = ExportationType.Instrument;
                    break;
                case "wave":
                    ExportationType = ExportationType.Wave;
                    break;
                default:
                    throw new Exception($"Invalid {nameof(SpectralSynthesizer.ExportationType)} as string.");
            }
        }

        #region Instrument SelectionBox

        /// <summary>
        /// Selects all elements of the <see cref="InstrumentsSelectionBox"/>.
        /// </summary>
        /// <param name="expotype">The type of selection box elements</param>
        private void SelectAllInstruments()
        {
            InstrumentsSelectionBox.SelectAllElements();
        }
        /// <summary>
        /// Deselects all elements of the <see cref="InstrumentsSelectionBox"/>.
        /// </summary>
        private void DeselectAllInstruments()
        {
            InstrumentsSelectionBox.DeselectAllElements();
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initilizes a new instance of the <see cref="ExportViewModel"/> class.
        /// </summary>
        /// <param name="window">The model <see cref="ExportWindow"/>.</param>
        public ExportViewModel(ExportWindow window)
        {
            Window = window;
            Wave = IoC.Get<AudioRendererModel>().Wave;
            // set up commands
            CloseCommand = new RelayCommand(() => Window.Close());
            SelectExportationTypeCommand = new ParameterizedRelayCommand((p) => SelectExportationType(p));
            SelectAllInstrumentsCommand = new RelayCommand(() => SelectAllInstruments());
            DeselectAllInstrumentsCommand = new RelayCommand(() => DeselectAllInstruments());
            BrowseCommand = new RelayCommand(() => Browse());
            InstrumentsSelectionBox.SelectionChanged += (e, v) => OnPropertyChanged("");
        }

        #endregion
    }
}
