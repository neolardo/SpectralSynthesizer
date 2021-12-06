using Microsoft.Xaml.Behaviors;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A helper class for monitoring the mouse poisiton inside a FrameworkElement
    /// </summary>
    public class MouseMoveBehaviour : Behavior<FrameworkElement>
    {

        /// <summary>
        /// The timer that updates the mouse positions
        /// </summary>
        private System.Timers.Timer MouseMoveTimer { get; set; } = new System.Timers.Timer();

        /// <summary>
        /// The delay between updates in milliseconds
        /// </summary>
        private int Delay => 20;

        /// <summary>
        /// The previous position of the mouse
        /// </summary>
        public (double x, double y) LastMousePosition
        {
            get { return ((double x, double y))GetValue(LastMousePositionProperty); }
            set { SetValue(LastMousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastMousePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastMousePositionProperty =
            DependencyProperty.Register("LastMousePosition", typeof((double x, double y)), typeof(MouseMoveBehaviour), new PropertyMetadata(default((double x, double y))));

        /// <summary>
        /// The current position of the mouse
        /// </summary>
        public (double x, double y) MousePosition
        {
            get { return ((double x, double y))GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        public static readonly DependencyProperty MousePositionProperty = DependencyProperty.Register(
            "MousePosition", typeof((double x, double y)), typeof(MouseMoveBehaviour), new PropertyMetadata(default((double x, double y))));


        /// <summary>
        /// A boolean that toggles the subscribtion to the mouse move event
        /// </summary>
        public bool IsSubscribedToMouseMove
        {
            get { return (bool)GetValue(IsSubscribedToMouseMoveProperty); }
            set { SetValue(IsSubscribedToMouseMoveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSubscribedToMouseMove.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSubscribedToMouseMoveProperty =
            DependencyProperty.Register("IsSubscribedToMouseMove", typeof(bool), typeof(MouseMoveBehaviour), new FrameworkPropertyMetadata(
            false, OnIsSubscribedToMouseMoveChanged));


        /// <summary>
        /// Called whenever the <see cref="IsSubscribedToMouseMove"/> is changed
        /// </summary>
        private static void OnIsSubscribedToMouseMoveChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var mouseNoveBehaviour = (MouseMoveBehaviour)source;
            bool value = (bool)e.NewValue;
            if ((bool)e.NewValue != (bool)e.OldValue)
            {
                // is the actual width is 0 the view is not updated yet
                if (value && mouseNoveBehaviour.AssociatedObject.ActualWidth > 0)
                {
                    mouseNoveBehaviour.StartMouseMoveTracking();
                }
                else
                {
                    mouseNoveBehaviour.EndMouseMoveTracking();
                }
            }
        }

        /// <summary>
        /// Starts a timer that copies the mouse position and sets the last positions.
        /// </summary>
        private void StartMouseMoveTracking()
        {
            var pos = Mouse.GetPosition(AssociatedObject);
            LastMousePosition = (pos.X, pos.Y);
            MouseMoveTimer.Enabled = true;
            MouseMoveTimer.Start();
        }

        /// <summary>
        /// Stops the timer that copies the mouse position
        /// </summary>
        private void EndMouseMoveTracking()
        {
            MouseMoveTimer.Stop();
            MouseMoveTimer.Enabled = false;
        }

        /// <summary>
        /// Sets the delta mouse positions
        /// </summary>
        private void MouseMoveTimerTask()
        {
            Dispatcher.Invoke(() =>
            {
                var pos = Mouse.GetPosition(AssociatedObject);
                MousePosition = (pos.X, pos.Y);
            });
        }

        /// <summary>
        /// Initializes a new instance of a mouse move behaviour
        /// </summary>
        public MouseMoveBehaviour()
        {
            MouseMoveTimer.Elapsed += (s, e) => MouseMoveTimerTask();
            MouseMoveTimer.Interval = Delay;
            MouseMoveTimer.Enabled = false;
        }

    }
}
