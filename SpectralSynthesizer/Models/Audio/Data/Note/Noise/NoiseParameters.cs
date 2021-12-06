
using Newtonsoft.Json;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents the parameters of the <see cref="Audio.Data.Noise"/> creation process.
    /// </summary>
    public class NoiseParameters : BaseModel
    {
        #region Parameters

        /// <summary>
        /// The sampling frequency.
        /// </summary>
        public Parameter<float> SamplingFrequency { get; } = new Parameter<float>(40f, 20f, 100f);


        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new NoiseParameters(this);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseParameters"/> class.
        /// </summary>
        public NoiseParameters() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseParameters"/> class by copying the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public NoiseParameters(NoiseParameters parameters)
        {
            SamplingFrequency.Value = parameters.SamplingFrequency.Value;
        }


        [JsonConstructor]
        public NoiseParameters(Parameter<float> samplingFrequency)
        {
            SamplingFrequency = samplingFrequency;
        }

        #endregion
    }
}
