using Newtonsoft.Json;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents the parameters of the <see cref="Audio.Data.Sinusoid"/> creation process.
    /// </summary>
    public class SinusoidParameters : BaseModel
    {
        #region Parameters

        /// <summary>
        /// Indicates whether the frequencies of the <see cref="Audio.Data.Sinusoid"/> are fixed or not.
        /// </summary>
        public bool AreFrequenciesFixed { get; set; } = false;

        /// <summary>
        /// The minimum decibel amplitude value of any <see cref="Audio.Data.SpectralUnit"/>.
        /// </summary>
        public Parameter<float> MinimumDecibelAmplitude { get; } = new Parameter<float>(ProjectModel.GlobalMinimumDecibel * 0.5f, ProjectModel.GlobalMinimumDecibel, 0);

        /// <summary>
        /// The maximum range in base 12 logarithmic values where two <see cref="Audio.Data.SpectralUnit"/>s are connected.
        /// </summary>
        public Parameter<float> ContinuationRange { get; } = new Parameter<float>(1f, 0.1f, 6f);

        /// <summary>
        /// The maximum amount of milliseconds a <see cref="Audio.Data.SpectralTrajectory"/> can sleep.
        /// </summary>
        public Parameter<int> MaximumSleepingTime { get; } = new Parameter<int>(100, 10, 1000);

        /// <summary>
        /// The minimum length of any <see cref="Audio.Data.SpectralTrajectory"/>.
        /// </summary>
        public Parameter<int> MinimumLength { get; } = new Parameter<int>(100, 10, 1000);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new SinusoidParameters(this);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new blank instance of the <see cref="SinusoidParameters"/> class.
        /// </summary>
        public SinusoidParameters() { }


        /// <summary>
        /// Initializes a new instance of the <see cref="SinusoidParameters"/> class by copying the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public SinusoidParameters(SinusoidParameters parameters)
        {
            AreFrequenciesFixed = parameters.AreFrequenciesFixed;
            MinimumDecibelAmplitude.Value = parameters.MinimumDecibelAmplitude.Value;
            ContinuationRange.Value = parameters.ContinuationRange.Value;
            MaximumSleepingTime.Value = parameters.MaximumSleepingTime.Value;
            MinimumLength.Value = parameters.MinimumLength.Value;
        }

        [JsonConstructor]
        public SinusoidParameters(Parameter<float> minimumDecibelAmplitude, Parameter<float> continuationRange, Parameter<int> maximumSleepingTime, Parameter<int> minimumLength)
        {
            MinimumDecibelAmplitude = minimumDecibelAmplitude;
            ContinuationRange = continuationRange;
            MaximumSleepingTime = maximumSleepingTime;
            MinimumLength = minimumLength;
        }

        #endregion
    }
}
