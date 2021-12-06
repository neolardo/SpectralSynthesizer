using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of a <see cref="Models.Audio.Data.MidiNote"/>.
    /// </summary>
    public class MidiNoteViewModel : BaseViewModel, IHoverable<MidiNoteViewModel>
    {
        #region Delegates and events

        /// <inheritdoc/>
        public event MouseOverChangedDelegate<MidiNoteViewModel> MouseOverChanged;

        #endregion

        #region Public Properties

        #region Model

        /// <summary>
        /// The model <see cref="MidiNote"/>.
        /// </summary>
        public MidiNote Model { get; set; }

        #endregion

        /// <summary>
        /// The amplitude of this note.
        /// </summary>
        public float Amplitude => Model.Amplitude;

        /// <summary>
        /// The horizontal position of this note in pixels.
        /// </summary>
        public double PositionX { get; set; }

        /// <summary>
        /// The vertical position of this note in pixels.
        /// </summary>
        public double PositionY => Model.NoteNumber * MidiViewViewModel.MidiNoteHeight;

        /// <summary>
        /// The length of this note in pixels.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of this note in pixels.
        /// </summary>
        public double Height => MidiViewViewModel.MidiNoteHeight;

        private bool _isMouseOver = false;

        /// <inheritdoc/>
        public bool IsMouseOver
        {
            get { return _isMouseOver; }
            set
            {
                if (value != _isMouseOver)
                {
                    _isMouseOver = value;
                    MouseOverChanged?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// The length of the selection of the note.
        /// </summary>
        public double SelectionWidth { get; set; }

        /// <summary>
        /// The selection margin of this note.
        /// </summary>
        public Thickness SelectionMargin { get; set; }

        /// <summary>
        /// The color of this midi item.
        /// </summary>
        public ApplicationColor Color { get; set; } = ApplicationColor.Theme;

        #endregion

        #region Commands

        /// <summary>
        /// Command for mouse enter.
        /// </summary>
        public ICommand MouseEnterCommand { get; set; }

        /// <summary>
        /// Command for mouse leave.
        /// </summary>
        public ICommand MouseLeaveCommand { get; set; }

        #endregion

        #region Methods


        /// <summary>
        /// Relocates the note.
        /// </summary>
        /// <param name="positionX">The new x poisiton of this note in pixels.</param>
        /// <param name="width">The new width of this note in pixels.</param>
        public void Relocate(double positionX, double width)
        {
            PositionX = positionX;
            Width = width;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiNoteViewModel"/> class.
        /// </summary>
        /// <param name="note">The midi note audio data model.</param>
        public MidiNoteViewModel(MidiNote note)
        {
            Model = note;
            MouseEnterCommand = new RelayCommand(() => IsMouseOver = true);
            MouseLeaveCommand = new RelayCommand(() => IsMouseOver = false);
        }
    }
}
