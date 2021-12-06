using System;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents an error when an enum's value is not expected.
    /// </summary>
    public class InvalidEnumValueException : Exception
    {
        public InvalidEnumValueException(Enum e) : base(e.GetType().ToString() + " is invalid.")
        {
        }
    }
}
