using SpectralSynthesizer.Models;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel for the main window.
    /// </summary>
    public class WindowViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The size of the inner content padding.
        /// </summary>
        public int InnerContentPaddingSize { get; set; } = 4;

        /// <summary>
        /// The inner content padding thickness.
        /// </summary>
        public Thickness InnerContentPadding => new Thickness(InnerContentPaddingSize);

        /// <summary>
        /// The minimum width the window can be resized to.
        /// </summary>
        public double WindowMinimumWidth { get; set; } = 900;

        /// <summary>
        /// The minimum height the window can be resized to.
        /// </summary>
        public double WindowMinimumHeight { get; set; } = 500;

        /// <summary>
        /// The minimum width the details panel can be resized to.
        /// </summary>
        public double MinimumDetailsWidth { get; set; } = 200;

        /// <summary>
        /// The maximum width the details panel can be resized to.
        /// </summary>
        public double MaximumDetailsWidth { get; set; } = 280;

        /// <summary>
        /// The height of the title in the window.
        /// </summary>
        public int TitleHeight { get; set; } = 30;

        /// <summary>
        /// The size that indicates how far away can we resize the window from the edges.
        /// </summary>
        public int ResizeBorder { get; set; } = 3;

        /// <summary>
        /// The size of the resize border with the outermargin.
        /// </summary>
        public Thickness ResizeBorderThickness { get { return new Thickness(ResizeBorder); } }

        /// <summary>
        /// The viewmodel of the <see cref="PreferencesWindow"/>.
        /// </summary>
        public PreferencesViewModel PreferencesViewModel { get; set; } = null;

        /// <summary>
        /// The viewmodel of the <see cref="ExportWindow"/>.
        /// </summary>
        public ExportViewModel ExportViewModel { get; set; } = null;

        /// <summary>
        /// The main window.
        /// </summary>
        private Window Window { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// The command to minimize the window
        /// </summary>
        public ICommand MinimizeCommand { get; set; }

        /// <summary>
        /// The command to maximize the window
        /// </summary>
        public ICommand MaximizeCommand { get; set; }

        /// <summary>
        /// The command to close the window
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        /// The command to close the window
        /// </summary>
        public ICommand NavigateCommand { get; set; }

        /// <summary>
        /// Command to open up the preferences window
        /// </summary>
        public ICommand PreferencesCommand { get; set; }

        /// <summary>
        /// Command to open up the export window
        /// </summary>
        public ICommand ExportCommand { get; set; }

        /// <summary>
        /// The command to create a blank project
        /// </summary>
        public ICommand VoidCommand { get; set; }

        /// <summary>
        /// The command to open a file
        /// </summary>
        public ICommand OpenCommand { get; set; }

        /// <summary>
        /// The command to save this project
        /// </summary>
        public ICommand SaveCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void Exit()
        {
            Computer.CancelEverything();
            //IoC.Get<ProjectViewModel>().UnloadAndDisposeEverything();
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the keydown events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void HandleKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ModelHistoryManager.Undo();
            }

            if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ModelHistoryManager.Redo();
            }

            if (e.Key == Key.Delete)
            {
                IoC.Get<ProjectViewModel>().OnDeletePressed();
            }

            if (e.Key == Key.Space || e.Key == Key.Return || e.Key == Key.Return)
            {
                IoC.Get<AudioPlayer>().TogglePlayback();
            }

            //e.Handled = true;
        }

        /// <summary>
        /// Handles the keyup events.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The evebt arguments.</param>
        private void HandleKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                Navigate();
            }

            // fixes the focus losing problem
            if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
            {
                Window.Focus();
            }

            //e.Handled = true;
        }

        /// <summary>
        /// Navigates between pages.
        /// </summary>
        public void Navigate()
        {
            if (IoC.Get<ProjectModel>().CurrentPage == ApplicationPage.InstrumentBuilder)
            {
                IoC.Get<ProjectModel>().NavigateTo(ApplicationPage.AudioRenderer);
            }
            else
            {
                IoC.Get<ProjectModel>().NavigateTo(ApplicationPage.InstrumentBuilder);
            }
        }

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        public void MaximizeWindow()
        {
            Window.WindowState ^= WindowState.Maximized;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowViewModel"/> class.
        /// </summary>
        /// <param name="window">The main <see cref="System.Windows.Window"/>.</param>
        public WindowViewModel(Window window)
        {
            Window = window;
            Window.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 4 * ResizeBorder;
            // Setting up the actaul commands for the window buttons
            MinimizeCommand = new RelayCommand(() => Window.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(() => MaximizeWindow());
            CloseCommand = new RelayCommand(() => Exit());
            NavigateCommand = new RelayCommand(() => Navigate());
            PreferencesCommand = new RelayCommand(() =>
            {
                PreferencesWindow pref = new PreferencesWindow();
                PreferencesViewModel = new PreferencesViewModel(pref);
                pref.DataContext = PreferencesViewModel;
                pref.ShowDialog();
                Window.Focus();
            });
            ExportCommand = new RelayCommand(() =>
            {
                ExportWindow expo = new ExportWindow();
                ExportViewModel = new ExportViewModel(expo);
                expo.DataContext = ExportViewModel;
                expo.ShowDialog();
                Window.Focus();
            });
            OpenCommand = new RelayCommand(() =>
            {
                OpenFileDialog openf = new OpenFileDialog();
                openf.Filter = $"SpectralSynthesizer project files (*.{ProjectModel.ProjectFileExtension})|*.{ProjectModel.ProjectFileExtension}|SpectralSynthesizer instrument files (*.{ProjectModel.InstrumentBundleFileExtension})|*.{ProjectModel.InstrumentBundleFileExtension}";
                if (openf.ShowDialog() == DialogResult.OK)
                {
                    IoC.Get<ProjectModel>().Import(openf.FileName);
                }
                Window.Focus();
            });
            VoidCommand = new RelayCommand(() =>
            {
                IoC.Get<ProjectModel>().CreateBlankProject();
            });
            SaveCommand = new RelayCommand(() =>
            {
                IoC.Get<ProjectModel>().SaveProject();
            });
            var resizer = new WindowResizer(Window);
            Window.KeyUp += HandleKeyUp;
            Window.KeyDown += HandleKeyDown;
        }

        #endregion
    }
}
