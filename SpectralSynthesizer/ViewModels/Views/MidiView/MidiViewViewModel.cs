using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="MidiView"/>.
    /// </summary>
    public class MidiViewViewModel : TimeDomainViewModel, IUndoableRedoable<Midi>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ModelSavedDelegate<Midi> ModelSaved;

        #endregion

        #region Properties

        #region Model

        private Midi _model;

        /// <summary>
        /// The model midi.
        /// </summary>
        public Midi Model
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
        /// The list of loaded midi notes.
        /// </summary>
        public ObservableCollection<MidiNoteViewModel> Notes { get; } = new ObservableCollection<MidiNoteViewModel>();

        /// <summary>
        /// The list of keyboard items.
        /// </summary>
        public ObservableCollection<MidiKeyboardItemViewModel> KeyboardItems { get; } = new ObservableCollection<MidiKeyboardItemViewModel>();

        /// <summary>
        /// The name of the loaded midi file.
        /// </summary>
        private string FileName => Model.FileName;

        /// <summary>
        /// Indicates whether the user can interract with the selected notes or not.
        /// </summary>
        public bool IsSelectionInteractable { get; set; } = true;

        #region Static

        /// <summary>
        /// The number of possible midi rows (representing notes).
        /// </summary>
        private static int RowNumber => ProjectModel.TotalDiscreteFrequencyNumber / ProjectModel.TonePerNote;

        /// <summary>
        /// The height of one midi row or note in pixels.
        /// </summary>
        public static double MidiNoteHeight => 8;

        /// <summary>
        /// The width of one keyboard row in pixels.
        /// </summary>
        public static double KeyboardWidth => 30;

        /// <summary>
        /// The height of the loaded midi in pixels.
        /// </summary>
        public static double MidiHeight => RowNumber * MidiNoteHeight;

        #endregion

        #region Highlight Note

        /// <summary>
        /// The highlighted note.
        /// </summary>
        private MidiNoteViewModel HighlightedNote { get; set; } = null;

        /// <summary>
        /// The text of the highlited note.
        /// </summary>
        public string HighlightedNoteString { get; set; } = "";

        /// <summary>
        /// The margin of the highlighted note text.
        /// </summary>
        public Thickness HighlightedNoteMargin { get; set; }

        /// <summary>
        /// True if the hghlighted note is in the selection range.
        /// </summary>
        public bool IsHighlightedNoteInSelectionRange { get; set; }

        /// <summary>
        /// The approx. width of the <see cref="HighlightedNoteString"/> in pixels.
        /// </summary>
        private double HighlightedNoteStringWidth => 20;

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command for right clicking of the content.
        /// </summary>
        public ICommand ContentRightClickCommand { get; set; }

        /// <summary>
        /// Command for left clicking on the content.
        /// </summary>
        public ICommand ContentLeftClickCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the <see cref="Midi"/> model.
        /// </summary>
        /// <param name="midi">The midi file.</param>
        public void LoadModel(Midi model)
        {
            Model = model;
            if (Model != null)
            {
                ConvertModelToViewModel();
                RefreshLocations();
                ColorizeItems();
                SelectAll();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Converts the midi to view model data.
        /// </summary>
        private void ConvertModelToViewModel()
        {
            Notes.Clear();
            foreach (MidiNote note in Model.Notes)
            {
                var item = new MidiNoteViewModel(note);
                item.MouseOverChanged += OnMouseOverNoteChanged;
                Notes.Add(item);
            }
        }

        /// <summary>
        /// Called when a note's mouse over property has changed.
        /// </summary>
        /// <param name="note">The note.</param>
        private void OnMouseOverNoteChanged(MidiNoteViewModel note)
        {
            if (IsSelectionInteractable)
            {
                //midi
                if (note.PositionX <= (Selection.End * Length) && (note.PositionX + note.Width) >= (Selection.Start * Length))
                {
                    note.Color = note.IsMouseOver ? ApplicationColor.ForegroundLight : ApplicationColor.Theme;
                }
                else
                {
                    note.Color = note.IsMouseOver ? ApplicationColor.Theme : ApplicationColor.BackgroundLight;
                }
                //keyboard
                var keyboardInd = KeyboardItems.Count - note.Model.NoteNumber - 1;
                if (note.IsMouseOver)
                    KeyboardItems[keyboardInd].Color = ApplicationColor.ForegroundLight; // this array is reversed
                else
                    KeyboardItems[keyboardInd].Color = KeyboardItems[keyboardInd].IsSharp ? ApplicationColor.BackgroundLight : ApplicationColor.ForegroundDark; // this array is reversed
                                                                                                                                                                //highlight
                if (note.IsMouseOver)
                {
                    HighlightedNote = note;
                    HighlightedNoteString = Computer.NoteToString(note.Model.NoteNumber);
                    double bottomMargin = (HighlightedNote.PositionY + MidiNoteHeight > MidiHeight) ? HighlightedNote.PositionY - 2 * MidiNoteHeight : HighlightedNote.PositionY + MidiNoteHeight;
                    HighlightedNoteMargin = new Thickness(HighlightedNote.PositionX, 0, 0, bottomMargin);
                    IsHighlightedNoteInSelectionRange = HighlightedNote.SelectionMargin.Left < HighlightedNoteStringWidth && HighlightedNote.SelectionWidth > Computer.CompareDelta;
                }
                else
                {
                    HighlightedNote = null;
                    HighlightedNoteString = "";
                }
            }
        }

        /// <summary>
        /// Refreshes the locations of the midi data.
        /// </summary>
        public void RefreshLocations()
        {
            Length = (Model.Length / 1000f) * PixelPerSecond;
            foreach (var note in Notes)
                note.Relocate(PixelPerSecond * (note.Model.Start / 1000f), PixelPerSecond * (note.Model.Length / 1000f));
            if (HighlightedNote != null)
            {
                HighlightedNoteMargin = new Thickness(HighlightedNote.PositionX, 0, 0, HighlightedNoteMargin.Bottom);
            }
        }

        /// <summary>
        /// Colorizes the midi items.
        /// </summary>
        public void ColorizeItems()
        {
            if (Notes == null || Notes.Count == 0)
                return;
            var selectionStart = Selection.Start * Length;
            var selectionEnd = Selection.End * Length;
            foreach (MidiNoteViewModel item in Notes)
            {
                // to refresh
                item.Color = ApplicationColor.BackgroundDark;
                if (item.PositionX <= selectionEnd && (item.PositionX + item.Width) >= selectionStart)
                {
                    double left = selectionStart - item.PositionX > 0 ? selectionStart - item.PositionX : 0;
                    item.SelectionWidth = (item.PositionX + item.Width) - selectionEnd > 0 ? item.Width - ((item.PositionX + item.Width) - selectionEnd) - left : item.Width - left;
                    item.SelectionMargin = new Thickness(left, 0, 0, 0);
                    item.Color = item.IsMouseOver ? ApplicationColor.ForegroundLight : ApplicationColor.Theme;
                }
                else
                {
                    item.SelectionWidth = 0;
                    item.SelectionMargin = new Thickness(0, 0, 0, 0);
                    item.Color = ApplicationColor.BackgroundLight;
                }
            }
            if (HighlightedNote != null)
            {
                IsHighlightedNoteInSelectionRange = HighlightedNote.SelectionMargin.Left < HighlightedNoteStringWidth && HighlightedNote.SelectionWidth > Computer.CompareDelta;
            }
        }

        /// <inheritdoc/>
        protected override void RefreshOnZoom()
        {
            RefreshLocations();
            ColorizeItems();
        }

        #region Selection

        /// <inheritdoc/>
        protected override void RefreshOnSelectionChanged() => ColorizeItems();

        /// <summary>
        /// Called on content left click.
        /// </summary>
        /// <param name="parameter">the content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentLeftClicked(object parameter)
        {
            var grid = parameter as Grid;
            SelectFrom(Mouse.GetPosition(grid).X);
        }

        /// <summary>
        /// Called on content right click.
        /// </summary>
        /// <param name="parameter">the content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentRightClicked(object parameter)
        {
            var grid = parameter as Grid;
            SelectTo(Mouse.GetPosition(grid).X);
        }

        #endregion

        #region Keyboard

        /// <summary>
        /// Creates the midi keyboard.
        /// </summary>
        public void CreateKeyboard()
        {
            KeyboardItems.Clear();
            for (int i = 0; i < RowNumber; i++)
            {
                bool isSharp = i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10;
                var item = new MidiKeyboardItemViewModel(isSharp);
                item.Color = item.IsSharp ? ApplicationColor.BackgroundLight : ApplicationColor.ForegroundDark;
                KeyboardItems.Insert(0, item);
            }
        }

        #endregion

        #endregion

        #region Constuctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiViewViewModel"/> class.
        /// </summary>
        public MidiViewViewModel()
        {
            CreateKeyboard();
            IoC.Get<ProjectModel>().GeneralSettings.ThemeChanged += ColorizeItems;
            ContentLeftClickCommand = new ParameterizedRelayCommand((param) => OnContentLeftClicked(param));
            ContentRightClickCommand = new ParameterizedRelayCommand((param) => OnContentRightClicked(param));
        }

        #endregion
    }
}