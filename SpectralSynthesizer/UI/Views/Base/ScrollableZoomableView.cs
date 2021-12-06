using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer.UI
{
    /// <summary>
    /// The base class of views that are scrollable and zoomable.
    /// </summary>
    public class ScrollableZoomableView : View
    {
        #region Delegates and Events

        public delegate void ControlScrollDelegate();

        /// <summary>
        /// Fires off when scrolling to left while holding the control and alt keys.
        /// </summary>
        public event ControlScrollDelegate AltControlScrollLeftDone;

        /// <summary>
        /// Fires off when scrolling to right while holding the control and alt keys.
        /// </summary>
        public event ControlScrollDelegate AltControlScrollRightDone;

        /// <summary>
        /// Fires off when scrolling to left while holding the control key.
        /// </summary>
        public event ControlScrollDelegate ControlScrollLeftDone;

        /// <summary>
        /// Fires ogg when scrolling to right while holding the control key.
        /// </summary>
        public event ControlScrollDelegate ControlScrollRightDone;

        #endregion

        #region Properties

        /// <summary>
        /// This view's main scrollviewer.
        /// </summary>
        protected ScrollViewer ScrollViewer { get; set; } = null;

        #endregion

        #region Methods

        /// <summary>
        /// Invokes the <see cref="AltControlScrollLeftDone"/> event
        /// </summary>
        protected void InvokeAltControlScrollLeftDone()
        {
            AltControlScrollLeftDone?.Invoke();
        }

        /// <summary>
        /// Invokes the <see cref="AltControlScrollRightDone"/> event
        /// </summary>
        protected void InvokeAltControlScrollRightDone()
        {
            AltControlScrollRightDone?.Invoke();
        }

        /// <summary>
        /// Invokes the <see cref="InvokeControlScrollLeftDone"/> event
        /// </summary>
        protected void InvokeControlScrollLeftDone()
        {
            ControlScrollLeftDone?.Invoke();
        }

        /// <summary>
        /// Invokes the <see cref="ControlScrollRightDone"/> event
        /// </summary>
        protected void InvokeControlScrollRightDone()
        {
            ControlScrollRightDone?.Invoke();
        }

        /// <summary>
        /// Called when the <see cref="ScrollViewer"/>'s preview mouse wheel event has been fired.
        /// </summary>
        /// <param name="sender">The sender scrollbar.</param>
        /// <param name="e">The event args.</param>
        protected virtual void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    AltControlScrollLeftDone?.Invoke();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    ScrollViewer.LineUp();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ControlScrollLeftDone?.Invoke();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    ScrollViewer.LineLeft();
                    ScrollViewer.LineLeft();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    ScrollViewer.LineUp();
                    ScrollViewer.LineUp();
                }
                else
                {
                    ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset - ScrollViewer.ActualWidth / 8);
                }
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    AltControlScrollRightDone?.Invoke();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    ScrollViewer.LineDown();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    ControlScrollRightDone?.Invoke();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    ScrollViewer.LineRight();
                    ScrollViewer.LineRight();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    ScrollViewer.LineDown();
                    ScrollViewer.LineDown();
                }
                else
                {
                    ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.HorizontalOffset + ScrollViewer.ActualWidth / 8);
                }
            }
            e.Handled = true;
        }

        /// <inheritdoc/>
        protected override void OnDataContextChanged()
        {
            base.OnDataContextChanged();
            var dc = DataContext as ScrollableZoomableViewViewModel;
            if (ScrollViewer != null)
            {
                //scroll
                ScrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;
                ScrollViewer.ScrollChanged += dc.OnScrollChanged;
                ScrollViewer.SizeChanged += dc.OnScrollViewerSizeChanged;
                //trigger the size change once to set the initial scroll properties
                dc.OnScrollViewerSizeChanged(ScrollViewer, null);
                dc.ScrollToOffset += (horizontal, vertical) =>
                {
                    ScrollViewer.ScrollToHorizontalOffset(horizontal);
                    ScrollViewer.ScrollToVerticalOffset(vertical);
                };
                //zoom
                ControlScrollLeftDone += dc.ZoomIn;
                ControlScrollRightDone += dc.ZoomOut;
                AltControlScrollLeftDone += dc.AltZoomIn;
                AltControlScrollRightDone += dc.AltZoomOut;
            }
        }

        #endregion

    }
}
