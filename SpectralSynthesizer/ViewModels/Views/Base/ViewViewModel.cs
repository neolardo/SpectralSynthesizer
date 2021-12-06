
namespace SpectralSynthesizer
{
    /// <summary>
    /// The abstract viewmodel class for any kind of basic view.
    /// </summary>
    public abstract class ViewViewModel : BaseViewModel
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing events when the <see cref="IsContentLoaded"/> property changes
        /// </summary>
        /// <param name="value">The new value of the <see cref="IsContentLoaded"/> property</param>
        public delegate void IsContentLoadedChangedDelegate(bool value);

        /// <summary>
        /// Fires off when the <see cref="IsContentLoaded"/> changes its value
        /// </summary>
        public event IsContentLoadedChangedDelegate IsContentLoadedChanged;

        /// <summary>
        /// A delegate for fireing off mouse related events.
        /// </summary>
        public delegate void MouseDelegate();

        /// <summary>
        /// Fires off when the center button of the mouse clicked this view.
        /// </summary>
        public event MouseDelegate MouseMiddleButtonClicked;

        #endregion

        #region Properties

        private bool _isContentLoaded;

        /// <summary>
        /// Indicates whether the content has been loaded to this view or not.
        /// </summary>
        public bool IsContentLoaded
        {
            get { return _isContentLoaded; }
            set
            {
                if (value != _isContentLoaded)
                {
                    _isContentLoaded = value;
                    IsContentLoadedChanged?.Invoke(value);
                }
            }
        }

        private bool _isInteractable = true;

        /// <summary>
        /// Indicates whether this view is interactable or not.
        /// </summary>
        public bool IsInteractable
        {
            get { return _isInteractable; }
            set
            {
                if (value != _isInteractable)
                {
                    _isInteractable = value;
                    OnIsInteractableChanged();
                }
            }
        }


        private double _length;

        /// <summary>
        /// The length of this view in pixels.
        /// </summary>
        public double Length
        {
            get { return _length; }
            set
            {
                double old = _length;
                _length = value;
                OnLengthChanged(old, _length);
            }
        }

        private double _height;

        /// <summary>
        /// The height of this view in pixels.
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                double old = _height;
                _height = value;
                OnHeightChanged(old, _height);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called after the <see cref="IsInteractable"/> property has changed.
        /// </summary>
        protected virtual void OnIsInteractableChanged() { }

        /// <summary>
        /// Called after the <see cref="Length"/> property has changed.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="Length"/> property.</param>
        /// <param name="newValue">The new value of the <see cref="Length"/> property.</param>
        protected virtual void OnLengthChanged(double oldValue, double newValue) { }

        /// <summary>
        /// Called after the <see cref="Height"/> property has changed.
        /// </summary>
        /// <param name="oldValue">The old value of the <see cref="Height"/> property.</param>
        /// <param name="newValue">The new value of the <see cref="Height"/> property.</param>
        protected virtual void OnHeightChanged(double oldValue, double newValue) { }

        /// <summary>
        /// Called after this view has been clicked by the middle mouse button.
        /// </summary>
        public void OnMiddleMouseClicked() { MouseMiddleButtonClicked?.Invoke(); }

        #endregion
    }
}
