
namespace SpectralSynthesizer.Models.Interfaces
{
    /// <summary>
    /// Delegate for calling a function on mouse move.
    /// </summary>
    public delegate void OnMouseMoveDelegate();

    /// <summary>
    /// Interface for classes that do something on the mouse move event.
    /// </summary>
    public interface IMouseMoveable
    {
        /// <summary>
        /// Called on mouse move.
        /// </summary>
        OnMouseMoveDelegate OnMouseMove { get; set; }

        /// <summary>
        /// The current position of the mouse.
        /// </summary>
        (double X, double Y) MousePosition { get; set; }

    }
}
