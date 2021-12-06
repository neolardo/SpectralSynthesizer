using NAudio.Wave;
using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.DataModels.Enums;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents the singleton audio player of this application.
    /// </summary>
    public class AudioPlayer
    {
        #region Properties

        /// <summary>
        /// The currently selected <see cref="AudioBufferWaveProvider"/>.
        /// </summary>
        private AudioBufferWaveProvider SelectedBuffer { get; set; } = null;

        /// <summary>
        /// A temporary <see cref="AudioBufferWaveProvider"/> storing the next buffer to be selected.
        /// </summary>
        private AudioBufferWaveProvider NextBuffer { get; set; } = null;

        /// <summary>
        /// The main audio output device for the application.
        /// It should not be disposed until the application exits, because we use only the <see cref="ProjectModel.StandardWaveFormat"/> for every kind of audio,
        /// so we can call <see cref="WaveOut.Init(IWaveProvider)"/> every time a new audio is loaded.
        /// </summary>
        private WaveOut WaveOut { get; set; } = new WaveOut();

        #endregion

        #region Methods

        #region Next Buffer

        /// <summary>
        /// Sets the given <see cref="AudioBufferWaveProvider"/> as the next buffer to play.
        /// </summary>
        /// <param name="buffer">The <see cref="AudioBufferWaveProvider"/>.</param>
        public void SetNextBuffer(AudioBufferWaveProvider buffer)
        {
            NextBuffer = buffer;
            if (WaveOut.PlaybackState == PlaybackState.Playing)
            {
                RequestStop();
            }
            else
            {
                SelectNextBuffer();
            }
        }

        /// <summary>
        /// Selects the <see cref="NextBuffer"/>.
        /// </summary>
        private void SelectNextBuffer()
        {
            if (SelectedBuffer != null)
            {
                SelectedBuffer.BufferModified -= RequestStop;
            }
            SelectedBuffer = NextBuffer;
            if (SelectedBuffer != null)
            {
                SelectedBuffer.BufferModified += RequestStop;
                NextBuffer = null;
                WaveOut.Init(SelectedBuffer);
            }
        }

        #endregion

        #region Playback

        /// <summary>
        /// Toggles the audio playback.
        /// </summary>
        public void TogglePlayback()
        {
            if (SelectedBuffer != null)
            {
                if (SelectedBuffer.State == AudioBufferState.Playing)
                {
                    RequestStop();
                }
                else
                {
                    Play();
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="WaveOut"/> to play music.
        /// </summary>
        private void Play()
        {
            if (SelectedBuffer != null)
            {
                SelectedBuffer.State = AudioBufferState.Playing;
                WaveOut.Play();
            }
        }
        /// <summary>
        /// Requests to stop the <see cref="WaveOut"/> from playing music.
        /// </summary>
        public void RequestStop()
        {
            if (SelectedBuffer != null && WaveOut.PlaybackState == PlaybackState.Playing)
            {
                WaveOut.Stop();
            }
        }

        /// <summary>
        /// Called whenever the playback has been stoped. The <see cref="WaveOut"/> cannot be used after that.
        /// </summary>
        /// <param name="sender">The sender <see cref="WaveOut"/>.</param>
        /// <param name="eventArgs">The stopped event arguments.</param>
        private void OnPlaybackStopped(object sender, StoppedEventArgs eventArgs)
        {
            SelectedBuffer.ResetPosition();
            if (NextBuffer != null)
            {
                SelectedBuffer.State = AudioBufferState.Loaded;
                SelectNextBuffer();
            }
            else
            {
                SelectedBuffer.State = AudioBufferState.Selected;
            }
        }

        #endregion

        #endregion

        #region Destructor

        /// <summary>
        /// Disposes the <see cref="WaveOut"/>.
        /// </summary>
        ~AudioPlayer()
        {
            WaveOut.PlaybackStopped -= OnPlaybackStopped;
            WaveOut.Dispose();
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioPlayer"/> class.
        /// </summary>
        public AudioPlayer()
        {
            WaveOut.PlaybackStopped += OnPlaybackStopped;
        }

        #endregion
    }
}
