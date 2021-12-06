using System.Windows.Input;

namespace SpectralSynthesizer.Models.Interfaces
{
    /// <summary>
    /// Delegate for fireing mouse click events when mouse pressing on an element.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <param name="element">The element.</param>
    public delegate void ElementMousePressedDelegate<T>(T element);

    /// <summary>
    /// Interface for left clickable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface ILeftClickable<T>
    {
        /// <summary>
        /// Fires off when left clicking this element.
        /// </summary>
        event ElementMousePressedDelegate<T> LeftClicked;

        /// <summary>
        /// Command for left click.
        /// </summary>
        ICommand LeftClickCommand { get; set; }
    }

    /// <summary>
    /// Interface for right clickable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface IRightClickable<T>
    {
        /// <summary>
        /// Fires off when right clicking this element.
        /// </summary>
        event ElementMousePressedDelegate<T> RightClicked;

        /// <summary>
        /// Command for right click.
        /// </summary>
        ICommand RightClickCommand { get; set; }
    }

    /// <summary>
    /// Interface for double left clickable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface IDoubleLeftClickable<T>
    {
        /// <summary>
        /// Fires off when double left clicking this element.
        /// </summary>
        event ElementMousePressedDelegate<T> DoubleLeftClicked;

        /// <summary>
        /// Command for double left click.
        /// </summary>
        ICommand DoubleLeftClickCommand { get; set; }
    }

    /// <summary>
    /// Interface for double right clickable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface IDoubleRightClickable<T>
    {
        /// <summary>
        /// Fires off when double right clicking this element.
        /// </summary>
        event ElementMousePressedDelegate<T> DoubleRightClicked;

        /// <summary>
        /// Command for double right click.
        /// </summary>
        ICommand DoubleRightClickCommand { get; set; }
    }

    /// <summary>
    /// Interface for mouse downable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface IMouseDownable<T>
    {
        /// <summary>
        /// Fires off when a mouse button is pressed down on this element.
        /// </summary>
        event ElementMousePressedDelegate<T> MouseDownDone;

        /// <summary>
        /// Command for mouse down.
        /// </summary>
        ICommand MouseDownCommand { get; set; }
    }

    /// <summary>
    /// Interface for mouse upable elements.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    interface IMouseUpable<T>
    {
        /// <summary>
        /// Fires off when a mouse button is released on this element.
        /// </summary>
        event ElementMousePressedDelegate<T> MouseUpDone;

        /// <summary>
        /// Command for mouse up.
        /// </summary>
        ICommand MouseUpCommand { get; set; }
    }
}
