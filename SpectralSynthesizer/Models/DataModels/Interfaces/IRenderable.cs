using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents an object with renderable audio.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Renders this <see cref="IRenderable"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate of the rendered audio.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio data.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token);
    }
}
