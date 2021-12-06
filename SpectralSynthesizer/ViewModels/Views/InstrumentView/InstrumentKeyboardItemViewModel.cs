using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The datacontext of one keyboard item in the <see cref="InstrumentView"/>
    /// </summary>
    public class InstrumentKeyboardItemViewModel : BaseViewModel, IHoverable<InstrumentKeyboardItemViewModel>, ILeftClickable<InstrumentKeyboardItemViewModel>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event MouseOverChangedDelegate<InstrumentKeyboardItemViewModel> MouseOverChanged;

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<InstrumentKeyboardItemViewModel> LeftClicked;

        #endregion

        #region Properties

        #region Model

        private Note _model;

        /// <summary>
        /// The note model.
        /// </summary>
        public Note Model
        {
            get { return _model; }
            set
            {
                if (_model != null)
                    _model.PropertyChanged -= OnPropertyChanged;
                _model = value;
                if (_model != null)
                    _model.PropertyChanged += OnPropertyChanged;
            }
        }

        #endregion

        /// <summary>
        /// True if this key is a black key on the piano
        /// </summary>
        public bool IsSharp { get; set; }

        /// <summary>
        /// The horizontal position of this keyboard item
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The Z index of this keyboard item
        /// </summary>
        public int Z => IsSharp ? 1 : 0;

        /// <summary>
        /// The outer height of this keyboard item
        /// </summary>
        public double OuterHeight { get; set; }

        /// <summary>
        /// The inner height of this keyboard item
        /// </summary>
        public double InnerHeight { get; set; }

        /// <summary>
        /// The outer width of this keyboard item
        /// </summary>
        public double OuterWidth { get; set; }

        /// <summary>
        /// The inner width of this keyboard item
        /// </summary>
        public double InnerWidth { get; set; }

        /// <summary>
        /// The color of this keyboard item
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.ForegroundDark;

        private bool _isMouseOver = false;

        /// <summary>
        /// Indicates wheter the mouse is over this line or not
        /// </summary>
        public bool IsMouseOver
        {
            get
            {
                return _isMouseOver;
            }
            set
            {
                if (value != _isMouseOver)
                {
                    _isMouseOver = value;
                    MouseOverChanged?.Invoke(this);
                }
            }
        }

        #endregion

        #region Commands

        /// <inheritdoc/>
        public ICommand MouseEnterCommand { get; set; }

        /// <inheritdoc/>
        public ICommand MouseLeaveCommand { get; set; }

        /// <inheritdoc/>
        public ICommand LeftClickCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Positions this keyboard item
        /// </summary>
        /// <param name="x">The horizontal position of this item</param>
        /// <param name="outerHeight">The inner outer height of this item</param>
        /// <param name="innerHeight">The inner height of this item</param>
        /// <param name="outerWidth">The outer width of this item</param>
        /// <param name="innerHeight">The inner width of this item</param>
        public void Position(double x, double outerHeight, double innerHeight, double outerWidth, double innerWidth)
        {
            X = x;
            OuterHeight = outerHeight;
            InnerHeight = innerHeight;
            OuterWidth = outerWidth;
            InnerWidth = innerWidth;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentKeyboardItemViewModel"/> class
        /// </summary>
        /// <param name="isSharp">The boolean that indicates wheter this key is sharp or not</param>
        public InstrumentKeyboardItemViewModel(bool isSharp)
        {
            IsSharp = isSharp;
            MouseEnterCommand = new RelayCommand(() => IsMouseOver = true);
            MouseLeaveCommand = new RelayCommand(() => IsMouseOver = false);
            LeftClickCommand = new RelayCommand(() => LeftClicked?.Invoke(this));
        }

        #endregion
    }
}
