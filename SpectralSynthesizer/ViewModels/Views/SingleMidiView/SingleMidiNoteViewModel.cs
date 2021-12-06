using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The view model of the <see cref="SingleMidiView"/>.
    /// </summary>
    public class SingleMidiNoteViewModel : BaseViewModel, IRightClickable<SingleMidiNoteViewModel>, IMouseDownable<SingleMidiNoteViewModel>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<SingleMidiNoteViewModel> RightClicked;

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<SingleMidiNoteViewModel> MouseDownDone;

        #endregion

        #region Properties

        #region Model

        /// <summary>
        /// The model <see cref="MidiNote"/>.
        /// </summary>
        public MidiNote Model { get; set; }

        #endregion

        /// <summary>
        /// The start of the midi in pixels.
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// The length of the midi in pixels.
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// The length of the midi in pixels.
        /// </summary>
        public double Height => SingleMidiViewViewModel.NoteHeight;

        /// <summary>
        /// The width of the adjustable area.
        /// </summary>
        public double AdjustableAreaWidth => SingleMidiViewViewModel.NoteAdjustableAreaWidth;

        /// <summary>
        /// The width of the selected part of the note.
        /// </summary>
        public double SelectionWidth { get; set; }

        /// <summary>
        /// The margin of the selected part of the note.
        /// </summary>
        public Thickness SelectionMargin { get; set; }

        /// <summary>
        /// The string representation of the note.
        /// </summary>
        public string NoteString { get; set; }

        /// <summary>
        /// Indicates whether this single midi note is selected or not.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Indicates whether the length adjustments modify the start of the note or the end of it.
        /// </summary>
        public bool IsAdjustLengthStart { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="NoteString"/> property is visible or not.
        /// </summary>
        public bool IsStringVisible => Length >= SingleMidiViewViewModel.MinimumVisibleNoteLength - Computer.CompareDelta;

        #endregion

        #region Commands

        /// <inheritdoc/>
        public ICommand RightClickCommand { get; set; }

        /// <inheritdoc/>
        public ICommand MouseDownCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the given <see cref="MidiNote"/> model.
        /// </summary>
        /// <param name="model">The <see cref="MidiNote"/> model.</param>
        public void LoadModel(MidiNote model)
        {
            Model = model;
            NoteString = Computer.NoteToString(Model.NoteNumber);
        }

        /// <summary>
        /// Called on deselecting this note.
        /// </summary>
        public void OnDeselect()
        {
            IsSelected = false;
            SetNoteNumberFromNoteString();
        }

        /// <summary>
        /// Sets the <see cref=MidiNote.NoteNumber"/> from the <see cref="NoteString"/>.
        /// </summary>
        private void SetNoteNumberFromNoteString()
        {
            int newNoteNumber = Model.NoteNumber;
            try
            {
                newNoteNumber = Computer.StringToNote(NoteString.ToLower());
            }
            catch (InvalidNoteFormatException)
            { }
            NoteString = Computer.NoteToString(newNoteNumber);
        }

        /// <summary>
        /// Relocates this note.
        /// </summary>
        /// <param name="start">The new start of the note in pixels.</param>
        /// <param name="length">The new length of the note in pixels.</param>
        public void Relocate(double start, double length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Called on right clicking.
        /// </summary>
        private void OnRichtClicked() => RightClicked?.Invoke(this);

        /// <summary>
        /// Called on mouse down.
        /// </summary>
        /// <param name="isStart">True if the start of the length should be adjusted, false is the end.</param>
        private void OnMouseDown(object isStart)
        {
            IsAdjustLengthStart = isStart.ToString() == "true";
            MouseDownDone?.Invoke(this);
        }

        #endregion

        #region Constuctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleMidiNoteViewModel"/> class.
        /// </summary>
        /// <param name="model">The <see cref="MidiNote"/> model of this note.</param>
        public SingleMidiNoteViewModel(MidiNote model)
        {
            LoadModel(model);
            RightClickCommand = new RelayCommand(() => OnRichtClicked());
            MouseDownCommand = new ParameterizedRelayCommand((param) => OnMouseDown(param));
        }

        #endregion
    }
}
