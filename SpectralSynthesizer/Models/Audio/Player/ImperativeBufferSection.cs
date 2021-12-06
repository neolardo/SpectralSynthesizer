using Newtonsoft.Json;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// A helper struct used for isolating the filled and unfilled part of <see cref="ImperativeBuffer"/>.
    /// </summary>
    public struct ImperativeBufferSection
    {
        #region Fields and Properties

        /// <summary>
        /// The start index of this section in the <see cref="ImperativeBuffer"/>.
        /// </summary>
        public int Start;

        /// <summary>
        /// The length of this section.
        /// </summary>
        public int Length;

        /// <summary>
        /// Indicates whether this section is filled or not.
        /// </summary>
        public bool IsFilled;

        #endregion

        #region Operators

        /// <summary>
        /// Equality operator for the <see cref="ImperativeBufferSection"/> struct.
        /// </summary>
        /// <param name="a">The first compareable <see cref="ImperativeBufferSection"/>.</param>
        /// <param name="b">The second compareable <see cref="ImperativeBufferSection"/>.</param>
        /// <returns>True if the two <see cref="ImperativeBufferSection"/>s are equal.</returns>
        public static bool operator ==(ImperativeBufferSection a, ImperativeBufferSection b)
        {
            return a.Start == b.Start && a.Length == b.Length && a.IsFilled == b.IsFilled;
        }

        /// <summary>
        /// Unequality operator for the <see cref="ImperativeBufferSection"/> struct.
        /// </summary>
        /// <param name="a">The first compareable <see cref="ImperativeBufferSection"/>.</param>
        /// <param name="b">The second compareable <see cref="ImperativeBufferSection"/>.</param>
        /// <returns>True if the two <see cref="ImperativeBufferSection"/>s are NOT equal.</returns>
        public static bool operator !=(ImperativeBufferSection a, ImperativeBufferSection b)
        {
            return !(a == b);
        }

        #endregion

        #region Constructor

        [JsonConstructor]
        /// <summary>
        /// Initializes a new instance of the <see cref="ImperativeBufferSection"/> struct.
        /// </summary>
        /// <param name="start">The start index of this section.</param>
        /// <param name="length">The length of this section.</param>
        /// <param name="isFilled">True if this section is filled, otherwise false.</param>
        public ImperativeBufferSection(int start, int length, bool isFilled)
        {
            Start = start;
            Length = length;
            IsFilled = isFilled;
        }

        #endregion
    }
}
