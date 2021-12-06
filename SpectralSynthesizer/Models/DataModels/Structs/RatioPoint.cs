using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents a generic value with a one dimensional normalized position.
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    public readonly struct RatioPoint<T>
    {
        #region Readonly Properties

        /// <summary>
        /// The value.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// The normalized position of this point. Ranges from 0.0 to 1.0.
        /// </summary>
        public double Position { get; }

        #endregion

        #region Constructor

        [JsonConstructor]
        /// <summary>
        /// Initializes a new instance of the <see cref="RatioPoint"/> struct.
        /// </summary>
        /// <param name="value">The data.</param>
        /// <param name="position"> The normalized position of this point. Ranges from 0.0 to 1.0.</param>
        public RatioPoint(T value, double position)
        {
            Value = value;
            Position = position;
        }

        #endregion
    }

    /// <summary>
    /// <see cref="RatioPoint.Position"/> comparer for the <see cref="RatioPoint"/> struct.
    /// Sorts in ascending form.
    /// </summary>
    public class RatioPointPositionComparer<T> : IComparer<RatioPoint<T>>
    {
        public int Compare(RatioPoint<T> x, RatioPoint<T> y)
        {
            return x.Position.CompareTo(y.Position);
        }
    }
}
