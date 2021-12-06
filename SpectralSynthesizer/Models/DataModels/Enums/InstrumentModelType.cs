using System;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents the type of models an <see cref="Audio.Data.Instrument"/> uses.
    /// </summary>
    [Flags]
    public enum InstrumentModelType
    {
        None = 0,
        Sinusoid = 1,
        Transient = 2,
        Noise = 4
    }
}
