
namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents a midi keyboard item
    /// </summary>
    public class FrequencyRulerItemViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// The string content of this ruler item.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The color of this line.
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.ForegroundDark;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyRulerItemViewModel"/> class.
        /// </summary>
        /// <param name="text">The string content of this ruler item.</param>
        public FrequencyRulerItemViewModel(string text)
        {
            Text = text;
        }

        #endregion
    }
}
