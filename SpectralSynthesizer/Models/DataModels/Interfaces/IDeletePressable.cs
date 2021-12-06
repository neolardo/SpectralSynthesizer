
namespace SpectralSynthesizer.Models.Interfaces
{
    /// <summary>
    /// Interface for views that can react to pressing the delete button.
    /// </summary>
    public interface IDeletePressable
    {
        /// <summary>
        /// Called after the pressing the delete key.
        /// </summary>
        void OnDeletePressed();
    }
}
