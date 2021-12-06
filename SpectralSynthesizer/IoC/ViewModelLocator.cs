
namespace SpectralSynthesizer
{
    /// <summary>
    /// Locates the viewmodels from <see cref="IoC"/> to use it in xaml files.
    /// </summary>
    public class ViewModelLocator
    {
        #region Singleton Instance

        /// <summary>
        /// Singleton instance of this locator.
        /// </summary>
        public static ViewModelLocator Instance { get; private set; } = new ViewModelLocator();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="SpectralSynthesizer.ProjectViewModel"/> from IoC.
        /// </summary>
        public ProjectViewModel ProjectViewModel => IoC.Get<ProjectViewModel>();

        /// <summary>
        /// Gets the <see cref="SpectralSynthesizer.InstrumentBuilderViewModel"/> from IoC.
        /// </summary>
        public InstrumentBuilderViewModel InstrumentBuilderViewModel => IoC.Get<InstrumentBuilderViewModel>();

        /// <summary>
        /// Gets the <see cref="SpectralSynthesizer.AudioRendererViewModel"/> from IoC.
        /// </summary>
        public AudioRendererViewModel AudioRendererViewModel => IoC.Get<AudioRendererViewModel>();

        #endregion

    }
}