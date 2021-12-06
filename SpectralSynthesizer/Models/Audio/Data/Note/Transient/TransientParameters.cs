using Newtonsoft.Json;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents the parameters of the <see cref="Audio.Data.Transient"/> creation process.
    /// </summary>
    public class TransientParameters : BaseModel
    {
        #region Parameters

        [JsonProperty]
        /// <summary>
        /// Indicates whether the transient is transposable.
        /// </summary>
        public bool IsTransposable { get; set; } = false;

        /// <summary>
        /// The strength of a detectable transient.
        /// </summary>
        public Parameter<float> Strength { get; } = new Parameter<float>(1.1f, 1.0f, 2.0f);

        /// <summary>
        /// The number of adjacent fft bins used in the calculation.
        /// </summary>
        public Parameter<int> AdjacencyNumber { get; } = new Parameter<int>(3, 1, 10);

        /// <summary>
        /// The minimum ratio of fft bins needed to flag a sample as a transient sample.
        /// </summary>
        public Parameter<float> FlagRatio { get; } = new Parameter<float>(0.15f, 0.1f, 0.5f);

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new TransientParameters(this);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientParameters"/> class.
        /// </summary>
        public TransientParameters() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientParameters"/> class by copying the given parameters.
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public TransientParameters(TransientParameters parameters)
        {
            IsTransposable = parameters.IsTransposable;
            Strength.Value = parameters.Strength.Value;
            AdjacencyNumber.Value = parameters.AdjacencyNumber.Value;
            FlagRatio.Value = parameters.FlagRatio.Value;
        }

        [JsonConstructor]
        public TransientParameters(bool isTransposable, Parameter<float> strength, Parameter<int> adjacencyNumber, Parameter<float> flagRatio)
        {
            IsTransposable = isTransposable;
            Strength = strength;
            AdjacencyNumber = adjacencyNumber;
            FlagRatio = flagRatio;
        }

        #endregion
    }
}
