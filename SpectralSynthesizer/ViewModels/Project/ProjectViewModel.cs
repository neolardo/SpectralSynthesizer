using SpectralSynthesizer.Models;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Contains every information of the currently loaded project.
    /// </summary>
    public class ProjectViewModel : BaseViewModel
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for keypress fireing events.
        /// </summary>
        public delegate void KeyPressedDelegate();

        #endregion

        #region Properties

        #region Static

        /// <summary>
        /// The minimum height of the splitable controls.
        /// </summary>
        public static double MinimumSplitHeight => 200;

        /// <summary>
        /// The minimum width of the splitable controls.
        /// </summary>
        public static double MinimumSplitWidth => 200;

        /// <summary>
        /// The minimum height of small splitable controls.
        /// </summary>
        public static double MinimumSmallSplitHeight => MinimumSplitHeight / 2.0;

        #endregion

        /// <summary>
        /// The theme of the application.
        /// </summary>
        public ApplicationTheme Theme => IoC.Get<ProjectModel>().GeneralSettings.Theme;

        /// <summary>
        /// The name of the loaded project.
        /// </summary>
        public string Name => IoC.Get<ProjectModel>().Name;

        /// <summary>
        /// The current page of the application.
        /// </summary>
        public ApplicationPage CurrentPage => IoC.Get<ProjectModel>().CurrentPage;

        /// <summary>
        /// The currently showing description text.
        /// </summary>
        public string Description { get; set; } = "Hover over something for more information.";

        #endregion

        #region Methods

        #region Key Press

        /// <summary>
        /// Called on pressing the delete key on the <see cref="MainWindow"/>.
        /// Forwards the deleting request to the correct destination.
        /// </summary>
        public void OnDeletePressed()
        {
            if (IoC.Get<ProjectModel>().CurrentPage == ApplicationPage.InstrumentBuilder)
                IoC.Get<InstrumentBuilderViewModel>().SingleMidiViewViewModel.OnDeletePressed();
        }

        #endregion

        /// <summary>
        /// Shows the description corresponding to the given parameter.
        /// </summary>
        /// <param name="desc">The abridgment of description.</param>
        public void ShowDescription(string desc)
        {
            switch (desc)
            {
                default:
                    Description = "";
                    break;
            }
        }


        #endregion

        #region Constructor

        /// <summary>
        /// Initiliazes a new instance of the <see cref="ProjectViewModel"/> class.
        /// </summary>
        public ProjectViewModel()
        {
            IoC.Get<ProjectModel>().PropertyChanged += OnPropertyChanged;
        }

        #endregion
    }
}
