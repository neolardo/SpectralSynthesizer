using SpectralSynthesizer.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    public class PreferencesViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The <see cref="PreferencesWindow"/>.
        /// </summary>
        private PreferencesWindow Window { get; set; }

        /// <summary>
        /// The title height of the window.
        /// </summary>
        public int TitleHeight { get; set; } = 28;

        /// <summary>
        /// The currently selected preference menu item.
        /// </summary>
        public PreferenceMenuItem SelectedPreferenceMenuItem { get; set; } = PreferenceMenuItem.General;

        /// <summary>
        /// The possible theme values
        /// </summary>
        public ComboBoxViewModel<NameableElementModel<ApplicationTheme>> ThemesComboBox { get; set; } = new ComboBoxViewModel<NameableElementModel<ApplicationTheme>>();

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string ProjectName { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Command for closing this window.
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// Command for selecting a menu item.
        /// </summary>
        public ICommand SelectPreferenceMenuItemCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method for selecting a menu item.
        /// </summary>
        /// <param name="item></param>
        public void SelectPreferenceMenuItem(object item)
        {
            switch ((string)item)
            {
                case "general":
                    SelectedPreferenceMenuItem = PreferenceMenuItem.General;
                    break;
                case "project":
                    SelectedPreferenceMenuItem = PreferenceMenuItem.Project;
                    break;
                default:
                    throw new Exception($"Invalid {nameof(PreferenceMenuItem)} as string.");
            }
        }

        /// <summary>
        /// Called after the <see cref="ThemesComboBox.SelectionChanged"/> event has been fired.
        /// </summary>
        /// <param name="element">The newly selected <see cref="NameableElementModel"/>.</param>
        private void OnThemeSelected(NameableElementModel<ApplicationTheme> element)
        {
            IoC.Get<ProjectModel>().GeneralSettings.Theme = element.Value;
            OnPropertyChanged("");
        }

        /// <summary>
        /// Method to save the preferences.
        /// </summary>
        private void Save()
        {
            IoC.Get<ProjectModel>().SaveGeneralSettings();
            if (String.IsNullOrEmpty(ProjectName) == false)
            {
                IoC.Get<ProjectModel>().Name = ProjectName;
            }
        }

        /// <summary>
        /// Method that is called when this window is closing.
        /// </summary>
        private void OnClose()
        {
            Save();
            Window.Close();
        }

        #region Init

        /// <summary>
        /// Initializes this class.
        /// </summary>
        private void Init()
        {
            ProjectName = IoC.Get<ProjectModel>().Name;
            InitCommands();
            InitThemesComboBox();
        }

        /// <summary>
        /// Initializes commands.
        /// </summary>
        private void InitCommands()
        {
            CloseCommand = new RelayCommand(() => OnClose());
            SelectPreferenceMenuItemCommand = new ParameterizedRelayCommand((p) => SelectPreferenceMenuItem(p));
        }

        /// <summary>
        /// Initializes the <see cref="ThemesComboBoxViewModel"/>.
        /// </summary>
        private void InitThemesComboBox()
        {
            var collection = new ObservableCollection<NameableElementModel<ApplicationTheme>>();
            foreach (int value in Enum.GetValues(typeof(ApplicationTheme)))
            {
                collection.Add(new NameableElementModel<ApplicationTheme>((ApplicationTheme)value, ((ApplicationTheme)value).ToString().ToLower()));
            }
            ThemesComboBox.LoadCollectionModel(collection);
            var selectedElement = collection.FirstOrDefault(_ => _.Value == IoC.Get<ProjectModel>().GeneralSettings.Theme);
            if (selectedElement != null)
            {
                ThemesComboBox.SelectElement(selectedElement);
            }
            ThemesComboBox.SelectionChanged += OnThemeSelected;
            IoC.Get<ProjectModel>().GeneralSettings.ThemeChanged += () => ThemesComboBox.OnPropertyChanged("");
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PreferencesViewModel"/> class.
        /// </summary>
        /// <param name="window">The <see cref="PreferencesWindow"/>.</param>
        public PreferencesViewModel(PreferencesWindow window)
        {
            Window = window;
            Init();
        }

        #endregion
    }
}
