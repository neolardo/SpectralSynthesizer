using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.DataModels.Enums;
using SpectralSynthesizer.Models.Interfaces;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="asd"/>.
    /// </summary>
    public class AudioBufferBorderViewModel : BaseViewModel, ILeftClickable<AudioBufferWaveProvider>
    {
        #region Delegates and Events

        /// <inheritdoc/>
        public event ElementMousePressedDelegate<AudioBufferWaveProvider> LeftClicked;

        #endregion

        #region Properties

        #region Model

        public AudioBufferWaveProvider _model = null;

        /// <summary>
        /// The <see cref="AudioBufferWaveProvider"/> model.
        /// </summary>
        public AudioBufferWaveProvider Model
        {
            get { return _model; }
            private set
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
        /// The state of audio buffer.
        /// </summary>
        public AudioBufferState State => Model == null ? AudioBufferState.Empty : Model.State;

        #endregion

        #region Commands

        /// <inheritdoc/>
        public ICommand LeftClickCommand { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the <see cref="AudioBufferWaveProvider"/> model.
        /// </summary>
        /// <param name="model">The <see cref="AudioBufferWaveProvider"/> model.</param>
        public void LoadModel(AudioBufferWaveProvider model)
        {
            Model = model;
        }

        /// <summary>
        /// Changes the <see cref="State"/> property to <see cref="AudioBufferState.Loaded"/> if it was selected or being played.
        /// </summary>
        public void Deselect()
        {
            if (State == AudioBufferState.Playing || State == AudioBufferState.Selected)
            {
                Model.State = AudioBufferState.Loaded;
            }
        }

        /// <summary>
        /// Selects this buffer.
        /// </summary>
        public void Select()
        {
            LeftClicked?.Invoke(Model);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initailizes a new instance of the <see cref="AudioBufferBorderViewModel"/>.
        /// </summary>
        public AudioBufferBorderViewModel()
        {
            LeftClickCommand = new RelayCommand(() => Select());
        }

        #endregion
    }
}
