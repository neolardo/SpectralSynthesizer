using NAudio.Wave;
using Newtonsoft.Json;
using SpectralSynthesizer.Models.DataModels.Enums;
using System;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents any audio buffer used by this application.
    /// </summary>
    public class AudioBufferWaveProvider : ImperativeBuffer, IWaveProvider
    {
        #region Properties

        /// <summary>
        /// The state of audio buffer.
        /// </summary>
        public AudioBufferState State { get; set; } = AudioBufferState.Empty;

        [JsonProperty]
        [JsonConverter(typeof(WaveFormatConverter))]
        /// <summary>
        /// The <see cref="NAudio.Wave.WaveFormat"/> of the data stored in the buffer.
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        /// The position of reading, which also equals to the number of already read bytes. 
        /// </summary>
        private int ReadPosition { get; set; } = 0;

        /// <summary>
        /// A flag used for silence.
        /// </summary>
        private bool OneTimeSilenceFlag = false;

        #endregion

        #region Methods

        /// <summary>
        /// Reads bytes from this buffer.
        /// </summary>
        /// <param name="buffer">The byte array to read to.</param>
        /// <param name="offset">The starting index of the data to read.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            lock (DataLock)
            {
                if (ReadPosition < Data.Length)
                {
                    var section = GetSection(ReadPosition / 4);
                    if (section.IsFilled)
                    {
                        int newReadPosition = ReadPosition + count;
                        newReadPosition = Math.Min(newReadPosition, section.Length * 4);
                        newReadPosition = Math.Min(newReadPosition, Selection.End * 4);
                        count = newReadPosition - ReadPosition;
                        Array.Copy(Data, ReadPosition, buffer, offset, count);
                        ReadPosition = newReadPosition;
                    }
                    else
                    {
                        if (!OneTimeSilenceFlag)
                        {
                            var silence = new byte[count];
                            Array.Copy(silence, 0, buffer, offset, count);
                            OneTimeSilenceFlag = true;
                        }
                        else
                        {
                            count = 0;
                            OneTimeSilenceFlag = false;
                        }
                    }
                }
                else
                {
                    count = 0;
                    OneTimeSilenceFlag = false;
                }
                return count;
            }
        }

        /// <summary>
        /// Resets the <see cref="ReadPosition"/> to the <see cref="ImperativeBuffer.Selection"/>'s start index.
        /// </summary>
        public void ResetPosition()
        {
            lock (DataLock)
            {
                ReadPosition = Selection.Start * 4;
            }
        }


        /// <inheritdoc/>
        public void Init(int length, WaveFormat waveFormat)
        {
            base.Init(length);
            WaveFormat = waveFormat;
            ResetPosition();
            if (State == AudioBufferState.Empty)
            {
                State = AudioBufferState.Loaded;
            }
        }

        /// <inheritdoc/>
        public override void ChangeSelection(double startRatio, double endRatio)
        {
            base.ChangeSelection(startRatio, endRatio);
            ResetPosition();
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            var copy = new AudioBufferWaveProvider(Data.Length);
            foreach (var section in Sections)
            {
                if (section.IsFilled)
                {
                    var dataChunk = new byte[section.Length];
                    Array.Copy(Data, section.Start, dataChunk, 0, dataChunk.Length);
                    copy.InsertData(dataChunk, section.Start);
                }
            }
            copy.ChangeSelection((double)Selection.Start / Data.Length, (double)Selection.End / Data.Length);
            copy.State = this.State;
            return copy;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioBufferWaveProvider"/> class.
        /// </summary>
        /// <param name="length">The length of this buffer in bytes.</param>
        public AudioBufferWaveProvider(int length) : base(length)
        { }

        [JsonConstructor]
        /// <summary>
        /// Initializes an empty <see cref="AudioBufferWaveProvider"/>. <see cref="ImperativeBuffer.Init(int)"/> should be called before using this buffer.
        /// </summary>
        public AudioBufferWaveProvider()
        { }

        #endregion
    }
}
