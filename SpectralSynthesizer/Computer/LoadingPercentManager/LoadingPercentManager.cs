
namespace SpectralSynthesizer
{
    public class LoadingPercentManager
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing events when the loading percent has changed.
        /// </summary>
        /// <param name="percentRatio">The new value of the loading percent ratio. Ranges from 0.0 to 1.0.</param>
        public delegate void LoadingPercentChangedDelegate(double percentRatio);

        /// <summary>
        /// Fires after the loading percent has changed.
        /// </summary>
        public event LoadingPercentChangedDelegate LoadingPercentChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The loading percent ratio. Ranges from 0.0 to 1.0.
        /// </summary>
        private double PercentRatio { get; set; }

        /// <summary>
        /// The loading percent's current step ratio. Ranges from 0.0 to 1.0.
        /// </summary>
        public double PercentStepRatio { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initilizes the percent properties.
        /// </summary>
        /// <param name="percentStepRatio"></param>
        public void Init(double percentStepRatio)
        {
            PercentStepRatio = percentStepRatio;
            PercentRatio = 0;
            LoadingPercentChanged?.Invoke(PercentRatio);
        }

        /// <summary>
        /// Loads one percent step.
        /// </summary>
        public void LoadStep()
        {
            PercentRatio += PercentStepRatio;
            LoadingPercentChanged?.Invoke(PercentRatio);
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingPercentManager"/> class.
        /// </summary>
        public LoadingPercentManager() { }

        #endregion
    }
}
