using Ninject;
using SpectralSynthesizer.Models;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A class for dependency injection.
    /// </summary>
    public static class IoC
    {
        #region Public Properies

        /// <summary>
        /// The Kernel for our IoC Kernel.
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();

        #endregion

        #region Construction

        /// <summary>
        /// Sets up the IoC Container, binds all information required and ready for use
        /// NOTE: Must be called as soon as the application starts up
        ///       to ensure all services can be found.
        /// </summary>
        public static void Setup()
        {
            BindAudioPlayer();
            BindModels();
            BindViewModels();
            LoadModelsForViewModels();
        }

        /// <summary>
        /// Binds the <see cref="AudioPlayer"/> class.
        /// </summary>
        private static void BindAudioPlayer()
        {
            Kernel.Bind<AudioPlayer>().ToConstant(new AudioPlayer());
        }

        /// <summary>
        /// Binds all singleton models.
        /// </summary>
        private static void BindModels()
        {
            Kernel.Bind<ProjectModel>().ToConstant(new ProjectModel());
        }

        /// <summary>
        /// Binds all singleton viewmodels.
        /// </summary>
        private static void BindViewModels()
        {
            Kernel.Bind<ProjectViewModel>().ToConstant(new ProjectViewModel());
            Kernel.Bind<InstrumentBuilderViewModel>().ToConstant(new InstrumentBuilderViewModel());
            Kernel.Bind<AudioRendererViewModel>().ToConstant(new AudioRendererViewModel());
        }

        /// <summary>
        /// Loads the models for the core viewmodels.
        /// </summary>
        private static void LoadModelsForViewModels()
        {
            Get<InstrumentBuilderViewModel>().LoadModel(Get<ProjectModel>().InstrumentBuilder);
            Get<AudioRendererViewModel>().LoadModel(Get<ProjectModel>().AudioRenderer);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets a service from the IoC of the specified type.
        /// </summary>
        /// <typeparam name="T">The type to get.</typeparam>
        /// <returns>The service.</returns>
        internal static T Get<T>()
        {
            return IoC.Kernel.Get<T>();
        }


        #endregion
    }
}
