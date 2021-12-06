using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="SingleMidiView"/>.
    /// </summary>
    /// <typeparam name="Midi">The <see cref="Midi"/> midi.</typeparam>
    public class SingleMidiViewViewModel : TimeDomainViewModel, IUndoableRedoable<Midi>, IMouseMoveable, IDeletePressable
    {
        #region Delegates and events

        /// <inheritdoc/>
        public event ModelSavedDelegate<Midi> ModelSaved;

        /// <inheritdoc/>
        public OnMouseMoveDelegate OnMouseMove { get; set; }

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
        /// The single midi notes.
        /// </summary>
        public ObservableCollection<SingleMidiNoteViewModel> Notes { get; set; } = new ObservableCollection<SingleMidiNoteViewModel>();

        /// <summary>
        /// The currently selected note.
        /// </summary>
        public SingleMidiNoteViewModel SelectedNote { get; set; }

        private MidiViewCursorType _cursorType = MidiViewCursorType.Selection;

        /// <summary>
        /// The current cursor type of the view.
        /// </summary>
        public MidiViewCursorType CursorType
        {
            get { return _cursorType; }
            set
            {
                if (value != _cursorType)
                {
                    _cursorType = value;
                    OnMidiViewCursorTypeChanged();
                }
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="SelectedNote"/>'s adjustment is turned on or not.
        /// </summary>
        public bool IsNoteLengthAdjustmentTurnedOn { get; set; }

        /// <summary>
        /// The threshold value of a note parameter in a specific case of note length adjusment.
        /// </summary>
        private double AdjustmentThreshold { get; set; }

        private (double X, double Y) _mousePosition;

        /// <summary>
        /// The current position of the mouse. Used when adjusting the length of a note.
        /// </summary>
        public (double X, double Y) MousePosition
        {
            get { return _mousePosition; }
            set
            {
                _mousePosition = value;
                OnMouseMove();
            }
        }

        /// <summary>
        /// The syncing object for mouse moving.
        /// </summary>
        private object MouseMoveSync { get; set; } = new object();

        #region Static

        /// <summary>
        /// The minimum width of a note whom <see cref="SingleMidiNoteViewModel.NoteString"/> is visible.
        /// </summary>
        public static double MinimumVisibleNoteLength => 24;

        /// <summary>
        /// The height of a note.
        /// </summary>
        public static double NoteHeight => 16;

        /// <summary>
        /// The height of a note.
        /// </summary>
        public static double NoteAdjustableAreaWidth => 4;

        /// <summary>
        /// The default note length of new notes in milliseconds.
        /// </summary>
        public static int DefaultNoteLength => 500;

        /// <summary>
        /// The default note amplitude of new notes.
        /// </summary>
        public static float DefaultNoteAmplitude => 0.5f;

        /// <summary>
        /// The default note number of new notes. C2.
        /// </summary>
        public static int DefaultNoteNumber => 24;

        #endregion

        #endregion

        #region Commands

        /// <summary>
        /// Command for content mouse up.
        /// </summary>
        public ICommand ContentMouseUpCommand { get; set; }

        /// <summary>
        /// Command for content left click.
        /// </summary>
        public ICommand ContentLeftClickCommand { get; set; }

        /// <summary>
        /// Command for content right click.
        /// </summary>
        public ICommand ContentRightClickCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes this view from a <see cref="TimeDomainModel"/>. This method should be called before the first <see cref="LoadModel(Midi)"/> method.
        /// </summary>
        /// <param name="wave"></param>
        public void InitFromTimeDomainModel(TimeDomainModel timeDomainModel)
        {
            if (timeDomainModel != null)
            {
                Length = timeDomainModel.Length / 1000f * PixelPerSecond;
            }
        }

        /// <summary>
        /// Loads the midi model.
        /// </summary>
        /// <param name="model">The <see cref="Midi"/> model.</param>
        public void LoadModel(Midi model)
        {
            Model = model;
            if (Model != null)
            {
                CreateViewModelData();
                RefreshLocations();
                RefreshSelectionParts();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Unloads the loaded model and clears this view.
        /// </summary>
        public void UnloadModel()
        {
            Model = null;
            IsContentLoaded = false;
        }

        /// <summary>
        /// Creates the viewmodel data from the model.
        /// </summary>
        private void CreateViewModelData()
        {
            Length = Model.Length / 1000f * PixelPerSecond;
            SelectedNote = null;
            Notes.Clear();
            for (int i = 0; i < Model.Notes.Count; i++)
            {
                CreateAndInsertNoteAt(i, Model.Notes[i]);
            }
        }

        /// <summary>
        /// Refreshes the locations of the midi data.
        /// </summary>
        private void RefreshLocations()
        {
            if (Model != null)
            {
                foreach (var note in Notes)
                    note.Relocate(PixelPerSecond * (note.Model.Start / 1000.0), PixelPerSecond * (note.Model.Length / 1000.0));
                Length = Model.Length / 1000f * PixelPerSecond;
            }
        }

        /// <inheritdoc/>
        protected override void RefreshOnZoom()
        {
            OnNoteLengthAdjustmentTurnedOff();
            RefreshLocations();
            RefreshSelectionParts();
        }

        /// <summary>
        /// Called after the <see cref="CursorType"/> property has changed.
        /// </summary>
        private void OnMidiViewCursorTypeChanged()
        {
            OnNoteLengthAdjustmentTurnedOff();
            DeselectNote();
        }


        #region Mouse 

        /// <summary>
        /// Called on content mouse up.
        /// </summary>
        private void OnContentMouseUp()
        {
            OnNoteLengthAdjustmentTurnedOff();
        }

        /// <summary>
        /// Called on content left click.
        /// </summary>
        /// <param name="parameter">The content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentLeftClicked(object parameter)
        {
            if (CursorType == MidiViewCursorType.Selection)
            {
                var grid = parameter as Grid;
                SelectFrom(Mouse.GetPosition(grid).X);
            }
        }

        /// <summary>
        /// Called on content right click.
        /// </summary>
        /// <param name="parameter">The content <see cref="Grid"/> as a boxed object.</param>
        private void OnContentRightClicked(object parameter)
        {
            var grid = parameter as Grid;
            double x = Mouse.GetPosition(grid).X;
            switch (CursorType)
            {
                case MidiViewCursorType.Selection:
                    SelectTo(x);
                    break;
                case MidiViewCursorType.Adjustment:
                    AddNoteAt(x);
                    break;
                default:
                    throw new InvalidOperationException("Invalid MidiViewCursorType.");
            };
        }

        /// <inheritdoc/>
        protected override void RefreshOnSelectionChanged()
        {
            base.RefreshOnSelectionChanged();
            RefreshSelectionParts();
        }

        /// <summary>
        /// Refreshes the selection areas of the <see cref="Notes"/>.
        /// </summary>
        private void RefreshSelectionParts()
        {
            if (Model != null)
            {
                double selectionStart = Length * Selection.Start;
                double selectionEnd = Length * Selection.End;
                foreach (var note in Notes)
                {
                    if (note.Start + note.Length > selectionStart)
                    {
                        note.SelectionMargin = new Thickness(selectionStart > note.Start ? selectionStart - note.Start : 0, 0, 0, 0);
                        if (note.Start < selectionEnd)
                            note.SelectionWidth = selectionEnd > note.Start + note.Length ? note.Length - note.SelectionMargin.Left : selectionEnd - (note.SelectionMargin.Left + note.Start);
                        else
                            note.SelectionWidth = 0;
                    }
                    else
                    {
                        note.SelectionMargin = new Thickness(0, 0, 0, 0);
                        note.SelectionWidth = 0;
                    }

                }
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Called on right clicking on a <see cref="SingleMidiNoteViewModel"/>.
        /// </summary>
        /// <param name="note">The note.</param>
        private void OnNoteRightClicked(SingleMidiNoteViewModel note)
        {
            if (IsInteractable && CursorType == MidiViewCursorType.Adjustment)
            {
                // toggle selection
                if (!note.IsSelected)
                    SelectNote(note);
                else
                    DeselectNote();
            }
        }

        /// <summary>
        /// Selects a note.
        /// </summary>
        /// <param name="note">The note.</param>
        private void SelectNote(SingleMidiNoteViewModel note)
        {
            DeselectNote();
            SelectedNote = note;
            note.IsSelected = true;
        }

        /// <summary>
        /// Deselects the selected note.
        /// </summary>
        private void DeselectNote()
        {
            if (SelectedNote != null)
            {
                SelectedNote.OnDeselect();
                RefreshSelectionParts();
                SaveModelOnSelectedNoteChanged();
                SelectedNote = null;
            }
        }

        #endregion

        #region Adjustment

        /// <summary>
        /// Called after mouse pressing on a note.
        /// </summary>
        /// <param name="note">The note.</param>
        private void OnNoteMouseDownDone(SingleMidiNoteViewModel note) => OnNoteLengthAdjustmentTurnedOn(note);

        /// <summary>
        /// Called when the note length adjustment is turned on.
        /// </summary>
        /// <param name="note">The note which's length will be adjusted.</param>
        private void OnNoteLengthAdjustmentTurnedOn(SingleMidiNoteViewModel note)
        {
            if (IsInteractable && CursorType == MidiViewCursorType.Adjustment)
            {
                SelectNote(note);
                int ind = Notes.IndexOf(note);
                if (note.IsAdjustLengthStart)
                {
                    AdjustmentThreshold = (ind - 1 >= 0) ? Notes[ind - 1].Start + Notes[ind - 1].Length + 1 : 0;
                    OnMouseMove = SetSelectedNoteStart;
                }
                else
                {
                    AdjustmentThreshold = (ind + 1 < Notes.Count) ? Notes[ind + 1].Start - 1 : Length;
                    OnMouseMove = SetSelectedNoteEnd;
                }
                IsNoteLengthAdjustmentTurnedOn = true;
            }
        }

        /// <summary>
        /// Called when the note length adjustment is turned off.
        /// </summary>
        private void OnNoteLengthAdjustmentTurnedOff()
        {
            if (IsNoteLengthAdjustmentTurnedOn)
            {
                IsNoteLengthAdjustmentTurnedOn = false;
                lock (MouseMoveSync)
                {
                    DeselectNote();
                }
            }
        }


        /// <summary>
        /// Called on mouse move to adjust the start of the <see cref="SelectedNote"/>.
        /// </summary>
        private void SetSelectedNoteStart()
        {
            lock (MouseMoveSync)
            {
                if (SelectedNote != null)
                {
                    double end = SelectedNote.Start + SelectedNote.Length;
                    if (MousePosition.X > AdjustmentThreshold)
                    {
                        double length = end - MousePosition.X;
                        if (length > MinimumVisibleNoteLength)
                        {
                            SelectedNote.Length = length;
                            SelectedNote.Start = MousePosition.X;
                        }
                        else if (end - MinimumVisibleNoteLength > AdjustmentThreshold)
                        {
                            SelectedNote.Start = end - MinimumVisibleNoteLength;
                            SelectedNote.Length = MinimumVisibleNoteLength;
                        }
                        else
                        {
                            SelectedNote.Length = end - AdjustmentThreshold;
                            SelectedNote.Start = AdjustmentThreshold;
                        }
                    }
                    else
                    {
                        SelectedNote.Length = end - AdjustmentThreshold;
                        SelectedNote.Start = AdjustmentThreshold;
                    }
                }
            }
        }

        /// <summary>
        /// Called on mouse move to adjust the end of the <see cref="SelectedNote"/>.
        /// </summary>
        private void SetSelectedNoteEnd()
        {
            lock (MouseMoveSync)
            {
                if (SelectedNote != null)
                {
                    if (MousePosition.X < AdjustmentThreshold)
                    {
                        double length = MousePosition.X - SelectedNote.Start;
                        if (length > MinimumVisibleNoteLength)
                            SelectedNote.Length = length;
                        else if (MinimumVisibleNoteLength + SelectedNote.Start < AdjustmentThreshold)
                            SelectedNote.Length = MinimumVisibleNoteLength;
                        else
                            SelectedNote.Length = AdjustmentThreshold - SelectedNote.Start;
                    }
                    else
                        SelectedNote.Length = AdjustmentThreshold - SelectedNote.Start;
                }
            }
        }

        #endregion

        #region Adding

        /// <summary>
        /// Adds a note at a certain location.
        /// </summary>
        /// <param name="x">The horizontal position of the new note's start.</param>
        private void AddNoteAt(double x)
        {
            if (IsNewNotePossibleAt(x))
            {
                double end = GetClosestStartDistanceFrom(x);
                var noteModel = new MidiNote(DefaultNoteNumber, DefaultNoteAmplitude, (int)(x / PixelPerSecond * 1000.0), Computer.ClampMax((int)((end - x) / PixelPerSecond * 1000.0), DefaultNoteLength));
                int index = 0;
                while (index < Notes.Count && Notes[index].Start < x)
                    index++;
                Model.Notes.Insert(index, noteModel);
                var note = CreateAndInsertNoteAt(index, noteModel);
                note.Relocate(PixelPerSecond * (note.Model.Start / 1000.0), PixelPerSecond * (note.Model.Length / 1000.0));
                SelectNote(note);
                RefreshSelectionParts();
                ModelSaved?.Invoke(Model);
            }
        }

        /// <summary>
        /// Returns true if a new note can be added with the given horizontal start position.
        /// </summary>
        /// <param name="x">The new note's start position.</param>
        /// <returns>True if a new note can be added with the given horizontal start position.</returns>
        private bool IsNewNotePossibleAt(double x)
        {
            if (x < 0 && x > Length)
                return false;
            foreach (var note in Notes)
            {
                if (x >= note.Start && x <= note.Start + note.Length)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the closest right hand side distance of a note's start position from the given horizontal position.
        /// </summary>
        /// <param name="x">The given horizontal position.</param>
        /// <returns>The closest right hand side distance of a note's start position from the given horizontal position.</returns>
        private double GetClosestStartDistanceFrom(double x)
        {
            double min = Length - x;
            foreach (var note in Notes)
            {
                if (note.Start - x > 0 && note.Start - x < min)
                    min = note.Start;
            }
            return min;
        }

        /// <summary>
        /// Inserts a <see cref="SingleMidiNoteViewModel"/> at the given index with the given <see cref="MidiNote"/> model.
        /// </summary>
        /// <param name="index">The index of the insertion.</param>
        /// <param name="model">The <see cref="MidiNote"/> model of the note.</param>
        /// <returns>The created <see cref="SingleMidiNoteViewModel"/>.</returns>
        private SingleMidiNoteViewModel CreateAndInsertNoteAt(int index, MidiNote model)
        {
            var note = new SingleMidiNoteViewModel(model);
            note.RightClicked += OnNoteRightClicked;
            note.MouseDownDone += OnNoteMouseDownDone;
            Notes.Insert(index, note);
            return note;
        }

        #endregion

        #region Deleting

        /// <inheritdoc/>
        public void OnDeletePressed()
        {
            RemoveSelectedNote();
        }

        /// <summary>
        /// Removes the <see cref="SelectedNote"/> from the <see cref="Notes"/> list.
        /// </summary>
        private void RemoveSelectedNote()
        {
            if (SelectedNote != null)
            {
                Model.Notes.Remove(SelectedNote.Model);
                Notes.Remove(SelectedNote);
                SelectedNote = null;
                if (Notes.Count == 0)
                    IsContentLoaded = false;
                ModelSaved?.Invoke(Model);
            }
        }
        #endregion

        #region Saving

        /// <summary>
        /// Saves the model when the selected note has changed.
        /// </summary>
        private void SaveModelOnSelectedNoteChanged()
        {
            if (SelectedNote != null)
            {
                int noteNumber = SelectedNote.Model.NoteNumber;
                try
                {
                    noteNumber = Computer.StringToNote(SelectedNote.NoteString);
                }
                catch (InvalidNoteFormatException)
                { }
                int index = Model.Notes.IndexOf(SelectedNote.Model);
                Model.Notes[index] = new MidiNote(noteNumber, SelectedNote.Model.Amplitude, (int)(SelectedNote.Start / PixelPerSecond * 1000f), (int)(SelectedNote.Length / PixelPerSecond * 1000f));
                SelectedNote.LoadModel(Model.Notes[index]);
                ModelSaved?.Invoke(Model);
            }
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleMidiViewViewModel"/> class.
        /// </summary>
        public SingleMidiViewViewModel()
        {
            ContentMouseUpCommand = new RelayCommand(() => OnContentMouseUp());
            ContentLeftClickCommand = new ParameterizedRelayCommand((param) => OnContentLeftClicked(param));
            ContentRightClickCommand = new ParameterizedRelayCommand((param) => OnContentRightClicked(param));
            OnMouseMove = SetSelectedNoteStart;
        }

        #endregion
    }
}
