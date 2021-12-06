
namespace SpectralSynthesizer
{
    /// <summary>
    /// A viewmodel for the labels in any kind of view.
    /// </summary>
    public class LabelViewModel : BaseViewModel
    {
        /// <summary>
        /// The label content.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// The horizontal position of this label.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelViewModel"/>.
        /// </summary>
        /// <param name="text">The content of the label</param>
        /// <param name="x">The horizontal position of the label</param>
        public LabelViewModel(string text, double x)
        {
            Text = text;
            X = x;
        }

    }
}
