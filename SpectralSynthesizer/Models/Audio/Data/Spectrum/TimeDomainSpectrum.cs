using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// A <see cref="Data.Spectrum"/> with a length in the time domain.
    /// </summary>
    public class TimeDomainSpectrum : TimeDomainModel, IRenderable
    {
        #region Properties

        /// <summary>
        /// The <see cref="Data.Spectrum"/>.
        /// </summary>
        public Spectrum Spectrum { get; set; }

        #endregion

        #region Methods

        #region Rendering

        /// <summary>
        /// Renders this <see cref="TimeDomainSpectrum"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            var buffer = new float[length];
            foreach (var spectral in Spectrum.Spectrals)
            {
                token.ThrowIfCancellationRequested();
                double phase = Computer.R.NextDouble() * Computer.SineWaveCache.Length;
                var sine = spectral.Render(sampleRate, length, ref phase);
                buffer = buffer.Zip(sine, (a, b) => a + b).ToArray();
            }
            return buffer;
        }

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy() => new TimeDomainSpectrum((Spectrum)this.Spectrum.GetDeepCopy(), this.Length);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeDomainSpectrum"/> class.
        /// </summary>
        /// <param name="spec">The <see cref="Data.Spectrum"/> which should be in the time domain.</param>
        /// <param name="length">The length of this <see cref="TimeDomainModel"/>in milliseconds.</param>
        public TimeDomainSpectrum(Spectrum spec, int length)
        {
            Spectrum = spec;
            Length = length;
        }

        #endregion
    }
}
