using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The datacontext of the <see cref="LoadingWindow"/>
    /// </summary>
    public class LoadingWindowViewModel : BaseViewModel
    {
        /// <summary>
        /// A delegate for fireing events from this <see cref="LoadingWindow"/>.
        /// </summary>
        public delegate void LoadingWindowDelegate();

        /// <summary>
        /// Fires off when the loading has been cancelled.
        /// </summary>
        public event LoadingWindowDelegate LoadingCancelled;

        #region Properties

        /// <summary>
        /// Private instance of the main window for statechanged and resize events.
        /// </summary>
        private Window Window { get; set; }

        /// <summary>
        /// The title of the window.
        /// </summary>
        public string TitleText { get; set; } = "loading";

        /// <summary>
        /// The complete ratio percent of the loading.
        /// </summary>
        public double Percent { get; set; } = 0.0;

        /// <summary>
        /// The size of the inner content padding.
        /// </summary>
        public int InnerContentPaddingSize { get; set; } = 4;

        /// <summary>
        /// The inner content padding thickness.
        /// </summary>
        public Thickness InnerContentPadding => new Thickness(InnerContentPaddingSize);

        /// <summary>
        /// The height of the title in the window.
        /// </summary>
        public double TitleHeight { get; set; } = 24;

        /// <summary>
        /// The height of the window.
        /// </summary>
        public double Height { get; set; } = 120;

        /// <summary>
        /// The width of the window.
        /// </summary>
        public double Width { get; set; } = 320;

        #endregion

        #region Commands

        /// <summary>
        /// Command to cancel the loading process.
        /// </summary>
        public ICommand CancelCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the loading window.
        /// </summary>
        public void Show()
        {
            Window.ShowDialog();
        }

        /// <summary>
        /// Resets the loading and hides the loading window.
        /// </summary>
        public void Close()
        {
            Window.Hide();
            Percent = 0.0;
        }
        /// <summary>
        /// Called on cancelling this loading process.
        /// </summary>
        private void OnCancel()
        {
            LoadingCancelled?.Invoke();
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingWindowViewModel"/> class.
        /// </summary>
        /// <param name="title">The title of this window.</param>
        /// <param name="loadingPercentManager">The <see cref="LoadingPercentManager"/> of this window.</param>
        public LoadingWindowViewModel(string title, LoadingPercentManager loadingPercentManager)
        {
            Window = new LoadingWindow();
            Window.DataContext = this;
            TitleText = title;
            CancelCommand = new RelayCommand(() => OnCancel());
            loadingPercentManager.LoadingPercentChanged += (value) => Percent = value;
        }

        #endregion

    }
}
