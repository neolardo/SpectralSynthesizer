using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.Audio.Data;
using SpectralSynthesizer.Models.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The datacontext of the <see cref="InstrumentView"/>.
    /// </summary>
    public class InstrumentViewViewModel : ViewViewModel, IUndoableRedoable<Instrument>
    {
        #region Delegates and events

        /// <summary>
        /// Delegate for <see cref="Note"/> selection events.
        /// </summary>
        /// <param name="note">The notenumber of the <see cref="Note"/>.</param>
        public delegate void SelectedNoteChangedDelegate(int noteNumber);

        /// <summary>
        /// Fires off when a <see cref="Note"/> of the loaded <see cref="Instrument"/> has been selected.
        /// </summary>
        public event SelectedNoteChangedDelegate SelectedNoteChanged;

        /// <inheritdoc/>
        public event ModelSavedDelegate<Instrument> ModelSaved;

        #endregion

        #region Properties

        #region Model

        private Instrument _model;

        /// <summary>
        /// The model instrument.
        /// </summary>
        public Instrument Model
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
        /// The instrument keyboard items.
        /// </summary>
        public ObservableCollection<InstrumentKeyboardItemViewModel> KeyboardItems { get; set; } = new ObservableCollection<InstrumentKeyboardItemViewModel>();

        /// <summary>
        /// The label ruler.
        /// </summary>
        public ObservableCollection<LabelViewModel> Labels { get; set; } = new ObservableCollection<LabelViewModel>();

        /// <summary>
        /// Represents the selected keyboard item.
        /// </summary>
        private InstrumentKeyboardItemViewModel SelectedKeyboardItem { get; set; }

        /// <summary>
        /// The name of the curretly loaded <see cref="Instrument"/>.
        /// </summary>
        public string Name { get; set; }

        private bool _isNameReadOnly = true;

        /// <summary>
        /// Indicates whether the name of the loaded <see cref="Instrument"/> is read only or not.
        /// </summary>
        public bool IsNameReadOnly
        {
            get { return _isNameReadOnly; }
            set
            {
                _isNameReadOnly = value;
                if (value)
                {
                    SaveModelOnNameChanged();
                }
            }
        }

        #region Keyboard Item Parameters

        /// <summary>
        /// The number of octaves the keyboard has.
        /// </summary>
        private static int OctaveCount => 9;

        /// <summary>
        /// The minimum height of an <see cref="InstrumentKeyboardItemViewModel"/>.
        /// </summary>
        private static double MinimumKeyboardItemHeight => 45;

        /// <summary>
        /// The maximum height of an <see cref="InstrumentKeyboardItemViewModel"/>.
        /// </summary>
        private static double MaximumKeyboardItemHeight => 60;

        private double _keyboardItemHeight = 50;

        /// <summary>
        /// The height of each (not sharp) keyboard item in pixels.
        /// </summary>
        public double KeyboardItemHeight
        {
            get { return _keyboardItemHeight; }
            set
            {
                if (value >= MinimumKeyboardItemHeight && value <= MaximumKeyboardItemHeight)
                {
                    _keyboardItemHeight = value;
                }
                else if (value < MinimumKeyboardItemHeight)
                {
                    _keyboardItemHeight = MinimumKeyboardItemHeight;
                }
                else if (value > MaximumKeyboardItemHeight)
                {
                    _keyboardItemHeight = MaximumKeyboardItemHeight;
                }
            }
        }

        /// <summary>
        /// The maximum outer width of each normal keyboard item in pixels.
        /// </summary>
        private static double KeyboardItemMaximumWidth => 16;

        private double _keyboardItemNormalWidth = 8;

        /// <summary>
        /// The outer width of each normal keyboard item in pixels.
        /// </summary>
        public double KeyboardItemNormalWidth
        {
            get { return _keyboardItemNormalWidth; }
            set
            {
                if (value > KeyboardItemMaximumWidth)
                {
                    _keyboardItemNormalWidth = KeyboardItemMaximumWidth;
                }
                else
                {
                    _keyboardItemNormalWidth = value;
                }
            }
        }

        /// <summary>
        /// The outer width of each sharp keyboard item in pixels.
        /// </summary>
        public double KeyboardItemSharpWidth => KeyboardItemNormalWidth * 0.7;

        /// <summary>
        /// The space between keys.
        /// </summary>
        public double KeyboardItemSpace => 0.1875 * KeyboardItemNormalWidth;

        #endregion

        /// <summary>
        /// The height of the label ruler area in pixels.
        /// </summary>
        public static double LabelHeight => 14;

        /// <summary>
        /// The height of the <see cref="InteractiveTextBox"/> containing the <see cref="Instrument"/>'s name.
        /// </summary>
        public static double InstrumentNameHeight => 18;

        /// <summary>
        /// The minimum height of the content.
        /// </summary>
        public static double MinimumHeight => LabelHeight + MinimumKeyboardItemHeight + InstrumentNameHeight + 20;

        /// <summary>
        /// The maximum height of the content.
        /// </summary>
        public static double MaximumHeight => LabelHeight + MaximumKeyboardItemHeight + InstrumentNameHeight + 20;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the instrument data.
        /// </summary>
        public void LoadModel(Instrument model)
        {
            Model = model;
            if (model != null)
            {
                ConvertModelToViewModelData();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Converts the model into viewmodel data.
        /// </summary>
        private void ConvertModelToViewModelData()
        {
            Name = Model.Name;
            CreateKeyboard();
            PositionKeyboardItems();
            CreateLabels();
        }

        /// <summary>
        /// Creates the keyboard.
        /// </summary>
        private void CreateKeyboard()
        {
            if (Model != null)
            {
                KeyboardItems.Clear();
                for (int i = 0; i < 12 * OctaveCount; i++)
                {
                    bool isSharp = (i % 12 == 1 || i % 12 == 3 || i % 12 == 6 || i % 12 == 8 || i % 12 == 10);
                    var item = new InstrumentKeyboardItemViewModel(isSharp);
                    item.Color = item.IsSharp ? ApplicationColor.BackgroundLight : ApplicationColor.ForegroundDark;
                    item.LeftClicked += OnKeyboardItemClicked;
                    item.MouseOverChanged += OnMouseOverKeyboardItem;
                    KeyboardItems.Add(item);
                }
                foreach (var note in Model.Notes)
                {
                    KeyboardItems[note.NoteNumber].Model = note;
                    KeyboardItems[note.NoteNumber].Color = ApplicationColor.Theme;
                }
            }
        }


        /// <summary>
        /// Positions the keyboard items.
        /// </summary>
        private void PositionKeyboardItems()
        {
            double[] normal = new double[]
            {
                KeyboardItemHeight,
                KeyboardItemHeight -KeyboardItemSpace * 2,
                KeyboardItemNormalWidth,
                KeyboardItemNormalWidth-KeyboardItemSpace
            };
            double[] sharp = new double[]
            {
                KeyboardItemHeight * 2.0 / 3.0,
                KeyboardItemHeight * 2.0 / 3.0 -KeyboardItemSpace * 2,
                KeyboardItemSharpWidth,
                KeyboardItemSharpWidth-KeyboardItemSpace *2
            };
            double wholeNoteX = 0;
            double sharpPos = KeyboardItemNormalWidth - KeyboardItemSharpWidth / 2.0;
            foreach (var item in KeyboardItems)
            {
                double x = 0;
                if (item.IsSharp)
                {
                    x = wholeNoteX - KeyboardItemNormalWidth + sharpPos;
                    item.Position(x, sharp[0], sharp[1], sharp[2], sharp[3]);
                }
                else
                {
                    x = wholeNoteX;
                    item.Position(x, normal[0], normal[1], normal[2], normal[3]);
                    wholeNoteX += KeyboardItemNormalWidth;
                }
            }
        }

        /// <summary>
        /// Creates the labels.
        /// </summary>
        private void CreateLabels()
        {
            if (Model == null)
                return;
            Labels.Clear();
            int octave = 12;
            for (int i = 0; i < KeyboardItems.Count; i += octave)
            {
                Labels.Add(new LabelViewModel((i / octave).ToString(), KeyboardItems[i].X));
            }
        }

        /// <summary>
        /// Positions the labels.
        /// </summary>
        private void PositionLabels()
        {
            int octave = 12;
            for (int i = 0; i < KeyboardItems.Count; i += octave)
            {
                Labels[i / octave].X = KeyboardItems[i].X;
            }
        }

        /// <summary>
        /// Calculates the keyboard's length and the height of the keyboard based on the <see cref="KeyboardIdealLengthHeightRatio"/> and the given new size.
        /// </summary>
        /// <param name="length">The new length of the keyboard.</param>
        /// <param name="height">The new height of the keyboard.</param>
        private void CalculateKeyboardParameters(double length, double height)
        {
            KeyboardItemNormalWidth = length / (OctaveCount * 7);
            Length = KeyboardItemNormalWidth * (OctaveCount * 7);
            KeyboardItemHeight = height;
        }


        /// <summary>
        /// Colorizes a keyboard item.
        /// </summary>
        /// <param name="item">The keyboard item.</param>
        private void ColorizeKeybordItem(InstrumentKeyboardItemViewModel item)
        {
            item.Color = ApplicationColor.BackgroundDark;
            if (item.IsMouseOver)
            {
                item.Color = ApplicationColor.ForegroundLight;
            }
            else
            {
                if (SelectedKeyboardItem == item)
                {
                    item.Color = ApplicationColor.ForegroundLight;
                }
                else if (item.Model != null)
                {
                    item.Color = ApplicationColor.Theme;
                }
                else
                {
                    item.Color = item.IsSharp ? ApplicationColor.BackgroundLight : ApplicationColor.ForegroundDark;
                }
            }
        }

        /// <summary>
        /// Refreshes the view when te content size has changed.
        /// </summary>
        /// <param name="sender">The content.</param>
        /// <param name="eventArgs">The event args.</param>
        public void OnContentSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            var grid = sender as Grid;
            CalculateKeyboardParameters(grid.ActualWidth, grid.ActualHeight - LabelHeight - InstrumentNameHeight);
            PositionKeyboardItems();
            PositionLabels();
        }


        /// <summary>
        /// Called when hovering over a keyboard item.
        /// </summary>
        /// <param name="item">The keyboard item.</param>
        private void OnMouseOverKeyboardItem(InstrumentKeyboardItemViewModel item)
        {
            ColorizeKeybordItem(item);
        }

        /// <summary>
        /// Called when clicking on a keyboard item.
        /// </summary>
        /// <param name="item">The keyboard item.</param>
        private void OnKeyboardItemClicked(InstrumentKeyboardItemViewModel item)
        {
            if (Model.Notes.Count > 0)
            {
                var old = SelectedKeyboardItem;
                SelectedKeyboardItem = item;
                if (old != null)
                    ColorizeKeybordItem(old);
                ColorizeKeybordItem(SelectedKeyboardItem);
                SelectedNoteChanged?.Invoke(KeyboardItems.IndexOf(item));
            }
        }

        /// <summary>
        /// Refreshes the colors of the <see cref="KeyboardItems"/>.
        /// </summary>
        private void RefreshColors()
        {
            if (Model != null)
            {
                foreach (var n in Model.Notes)
                {
                    ColorizeKeybordItem(KeyboardItems[n.NoteNumber]);
                }
            }
        }

        #region Saving

        /// <summary>
        /// Saves the model after the loaded <see cref="Instrument"/>'s name has been changed.
        /// </summary>
        private void SaveModelOnNameChanged()
        {
            Name = Model.TrySetName(Name);
            ModelSaved?.Invoke(Model);
        }

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentViewViewModel"/> class.
        /// </summary>
        public InstrumentViewViewModel()
        {
            IoC.Get<ProjectModel>().GeneralSettings.ThemeChanged += RefreshColors;
        }

        #endregion
    }
}
