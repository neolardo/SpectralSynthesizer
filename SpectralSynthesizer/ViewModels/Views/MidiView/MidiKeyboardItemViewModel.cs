
namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents a midi keyboard item.
    /// </summary>
    public class MidiKeyboardItemViewModel : BaseViewModel
    {
        #region Properties

        /// <summary>
        /// True if this key is a black key on the piano.
        /// </summary>
        public bool IsSharp { get; set; }

        /// <summary>
        /// The height of one midi row.
        /// </summary>
        public double Height => MidiViewViewModel.MidiNoteHeight;

        /// <summary>
        /// The keyboard width of this midi row.
        /// </summary>
        public double Width => IsSharp ? MidiViewViewModel.KeyboardWidth / 2.0 : MidiViewViewModel.KeyboardWidth;

        /// <summary>
        /// The color of this midi row.
        /// </summary>
        public ApplicationColor Color { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiKeyboardItemViewModel"/> class.
        /// </summary>
        /// <param name="isSharp">The boolean that indicates wheter this key is sharp or not.</param>
        public MidiKeyboardItemViewModel(bool isSharp)
        {
            IsSharp = isSharp;
        }

        #endregion
    }
}
