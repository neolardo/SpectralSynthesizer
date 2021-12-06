using System.Windows.Input;

namespace SpectralSynthesizer.Models.Interfaces
{
    /// <summary>
    /// Delegate for indicating that an element's IsMouseOver property has changed.
    /// </summary>
    /// <param name="element">An instance of the element.</param>
    public delegate void MouseOverChangedDelegate(IHoverable element);

    /// <summary>
    /// Interface for all kind of elements which are hoverable.
    /// </summary>
    public interface IHoverable
    {
        /// <summary>
        /// Fires off when the <see cref="IsMouseOver"/> property has changed.
        /// </summary>
        event MouseOverChangedDelegate MouseOverChanged;

        /// <summary>
        /// True if the mouse is over this element.
        /// </summary>
        bool IsMouseOver { get; set; }

        /// <summary>
        /// Command for mouse enter.
        /// </summary>
        ICommand MouseEnterCommand { get; set; }

        /// <summary>
        /// Command for mouse leave.
        /// </summary>
        ICommand MouseLeaveCommand { get; set; }

    }

    /// <summary>
    /// Delegate for indicating that an element's IsMouseOver property has changed.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <param name="element">An instance of the element.</param>
    public delegate void MouseOverChangedDelegate<T>(T element);

    /// <summary>
    /// The generic version of the <see cref="IHoverable"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    public interface IHoverable<T>
    {
        /// <summary>
        /// Fires off when the <see cref="IsMouseOver"/> property has changed.
        /// </summary>
        event MouseOverChangedDelegate<T> MouseOverChanged;

        /// <summary>
        /// True if the mouse is over this element.
        /// </summary>
        bool IsMouseOver { get; set; }

        /// <summary>
        /// Command for mouse enter.
        /// </summary>
        ICommand MouseEnterCommand { get; set; }

        /// <summary>
        /// Command for mouse leave.
        /// </summary>
        ICommand MouseLeaveCommand { get; set; }

    }
}
