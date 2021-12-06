using SpectralSynthesizer.Models.Audio.Data;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents the a type of windowing used in <see cref="Spectrum"/> creation.
    /// </summary>
    public enum WindowingType
    {
        Full,
        HalfDescending,
        HalfAscending,
    }
}
