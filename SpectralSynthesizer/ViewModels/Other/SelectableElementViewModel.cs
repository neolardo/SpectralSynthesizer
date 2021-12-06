using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of any selectable element.
    /// </summary>
    /// <typeparam name="T">The type of the element's model.</typeparam>
    public class SelectableElementViewModel<T> : BaseViewModel where T : BaseModel, INameable
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing off event when the <see cref="IsSelected"/> property of this element has changed.
        /// </summary>
        /// <param name="element">This element.</param>
        public delegate void SelectionChangedDelegate(SelectableElementViewModel<T> element);

        /// <summary>
        /// Fires off when the <see cref="IsSelected"/> property of this element has changed.
        /// </summary>
        public event SelectionChangedDelegate SelectionChanged;

        #endregion

        #region Properties

        #region Model

        private T _model = null;

        /// <summary>
        /// The <see cref="T"/> model.
        /// </summary>
        public T Model
        {
            get { return _model; }
            set
            {
                if (_model != null)
                {
                    _model.PropertyChanged -= OnPropertyChanged;
                }
                _model = value;
                if (_model != null)
                {
                    _model.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        #endregion

        /// <summary>
        /// The name of this element.
        /// </summary>
        public string Name => Model.Name;

        /// <summary>
        /// Indicates whether this element is selected or not.
        /// </summary>
        public bool IsSelected { get; set; } = false;

        #endregion

        #region Commands

        /// <summary>
        /// Command for selecting this element.
        /// </summary>
        public ICommand SelectCommand { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectableElementViewModel{T}"/> class.
        /// </summary>
        /// <param name="model">The <see cref="T"/> model.</param>
        public SelectableElementViewModel(T model)
        {
            Model = model;
            SelectCommand = new RelayCommand(() => SelectionChanged(this));
        }

        #endregion
    }
}
