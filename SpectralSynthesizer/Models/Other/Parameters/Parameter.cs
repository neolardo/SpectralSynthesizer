using System;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents a parameter.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public class Parameter<T> : BaseModel where T : struct, IConvertible
    {
        #region Properties

        /// <summary>
        /// The minimum value of the parameter.
        /// </summary>
        public T Minimum { get; }

        /// <summary>
        /// The maximum value of the parameter.
        /// </summary>
        public T Maximum { get; }

        /// <summary>
        /// The current value of the parameter.
        /// </summary>
        public T Value { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new Parameter<T>(Value, Minimum, Maximum);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value of this parameter.</param>
        /// <param name="minimum">The minimum value of this parameter.</param>
        /// <param name="maximum">The maximum value of this parameter.</param>
        public Parameter(T initialValue, T minimum, T maximum)
        {
            Value = initialValue;
            Minimum = minimum;
            Maximum = maximum;
        }

        #endregion
    }
}
