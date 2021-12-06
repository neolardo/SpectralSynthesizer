using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents a MIDI note.
    /// </summary>
    public readonly struct MidiNote
    {
        #region Readonly Properties

        /// <summary>
        /// The amplitude of this note from 0.0 to 1.0.
        /// </summary>
        public float Amplitude { get; }

        /// <summary>
        /// The starting position of this note in milliseconds.
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// The length of this note in milliseconds.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// The note number of this note.
        /// </summary>
        public int NoteNumber { get; }

        #endregion


        #region Methods

        /// <summary>
        /// Renders this <see cref="MidiNote"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            int noteStart = (int)(Start * (sampleRate / 1000f));
            int noteEnd = Computer.ClampMax(noteStart + (int)(Length * (sampleRate / 1000f)), length);
            int noteLength = noteEnd - noteStart;
            double phase = Computer.R.NextDouble() * Computer.SineWaveCache.Length;
            return new SpectralUnit(Computer.DecibelToPCMAmplitude(Computer.PCMAmplitudeToDecibel(Amplitude) - ProjectModel.MaximumSingleDecibelAmplitudeDelta), Computer.NoteToFrequency(NoteNumber)).Render(sampleRate, noteLength, ref phase).Fade();
        }

        #endregion

        #region Constructor

        [JsonConstructor]
        /// <summary>
        /// Initializes a new instance of the <see cref="MidiNote"/> class.
        /// </summary>
        /// <param name="noteNumber">The note number.</param>
        /// <param name="amplitude">The amplitude.</param>
        /// <param name="start">The start of the note in milliseconds.</param>
        /// <param name="length">The end of the note in milliseconds.</param>
        public MidiNote(int noteNumber, float amplitude, int start, int length)
        {
            NoteNumber = noteNumber;
            Amplitude = amplitude;
            Start = start;
            Length = length;
        }

        #endregion
    }

    /// <summary>
    /// <see cref="MidiNote.NoteNumber"/> comparer for the <see cref="MidiNote"/> class.
    /// </summary>
    public class MidiNoteNoteNumberComparer : IComparer<MidiNote>
    {
        public int Compare(MidiNote x, MidiNote y)
        {
            return (x.NoteNumber == y.NoteNumber) ? x.Start.CompareTo(y.Start) : x.NoteNumber.CompareTo(y.NoteNumber);
        }
    }

    /// <summary>
    /// <see cref="MidiNote.Start"/> time ascending comparer for the <see cref="MidiNote"/> class.
    /// </summary>
    public class MidiNoteStartComparer : IComparer<MidiNote>
    {
        public int Compare(MidiNote x, MidiNote y)
        {
            return x.Start.CompareTo(y.Start);
        }
    }

}
