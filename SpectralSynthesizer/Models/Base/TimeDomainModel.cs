using Newtonsoft.Json;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// An abstract class for all models whom data is in the time domain.
    /// </summary>
    public abstract class TimeDomainModel : BaseModel
    {
        [JsonProperty]
        /// <summary>
        /// The length of the model in milliseconds.
        /// </summary>
        public int Length { get; protected set; }
    }
}
