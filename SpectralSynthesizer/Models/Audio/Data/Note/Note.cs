using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents a single note of an <see cref="Instrument"/>.
    /// </summary>
    public class Note : TimeDomainModel, IRenderable
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The sinousoid model.
        /// </summary>
        public Sinusoid Sinusoid { get; private set; }

        [JsonProperty]
        /// <summary>
        /// The transient model.
        /// </summary>
        public Transient Transient { get; private set; }

        [JsonProperty]
        /// <summary>
        /// The noise model.
        /// </summary>
        public Noise Noise { get; private set; }

        [JsonProperty]
        /// <summary>
        /// The note number.
        /// </summary>
        public int NoteNumber { get; private set; }

        #endregion

        #region Methods

        #region Rendering

        /// <summary>
        /// Renders this <see cref="Note"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            var buffer = new float[length];
            int noteLength = Computer.ClampMax((int)((Length / 1000.0) * sampleRate), length);
            if (Sinusoid != null)
            {
                var sinusoidWave = Sinusoid.Render(sampleRate, noteLength, token);
                Array.Copy(buffer.Zip(sinusoidWave, (a, b) => a + b).ToArray(), buffer, noteLength);
            }
            if (Transient != null)
            {
                var transientWave = Transient.Wave.Data.Take(noteLength).ToArray();
                Array.Copy(buffer.Zip(transientWave, (a, b) => a + b).ToArray(), buffer, Math.Min(noteLength, transientWave.Length));
            }
            if (Noise != null)
            {
                var noiseWave = Noise.Wave.Data.Take(noteLength).ToArray();
                Array.Copy(buffer.Zip(noiseWave, (a, b) => a + b).ToArray(), buffer, Math.Min(noteLength, noiseWave.Length));
            }
            return buffer;
        }

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return null;
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        public Note() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class by interpolating between two existing <see cref="Note"/>s.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="noteNumber">The note number.</param>
        /// <param name="a">The first <see cref="Note"/>.</param>
        /// <param name="b">The second <see cref="Note"/>.</param>
        /// <param name="token">The second <see cref="CancellationToken"/> to cancel this function.</param>
        public Note(int noteNumber, Note a, Note b, CancellationToken token)
        {
            NoteNumber = noteNumber;
            float lerpValue = (noteNumber - a.NoteNumber) / (float)(b.NoteNumber - a.NoteNumber);
            Length = Computer.Lerp(a.Length, b.Length, lerpValue);
            float aFreqScale = Computer.NoteToFrequency(NoteNumber) / Computer.NoteToFrequency(a.NoteNumber);
            float bFreqScale = Computer.NoteToFrequency(NoteNumber) / Computer.NoteToFrequency(b.NoteNumber);
            Sinusoid = a.Sinusoid == null ? null : new Sinusoid(a.Sinusoid, b.Sinusoid, lerpValue, aFreqScale, bFreqScale, token);
            Transient = a.Transient == null ? null : new Transient(a.Transient, b.Transient, lerpValue, aFreqScale, bFreqScale, token);
            Noise = a.Noise == null ? null : new Noise(a.Noise, b.Noise, lerpValue, NoteNumber - a.NoteNumber, NoteNumber - b.NoteNumber, token);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/> containing the audio of the <see cref="Note"/>.</param>
        /// <param name="midiNote">The <see cref="MidiNote"/>.</param>
        /// <param name="type">The <see cref="InstrumentModelType"/>.</param>
        /// <param name="sinusoidParameters">The <see cref="SinusoidParameters"/>.</param>
        /// <param name="transientParameters">The <see cref="TransientParameters"/>.</param>
        /// <param name="noiseParameters">The <see cref="NoiseParameters"/>.</param>
        /// <param name="token"></param>
        public Note(Wave wave, MidiNote midiNote, InstrumentModelType type, SinusoidParameters sinusoidParameters, TransientParameters transientParameters, NoiseParameters noiseParameters, CancellationToken token)
        {
            NoteNumber = midiNote.NoteNumber;
            Length = midiNote.Length;
            wave.Data = wave.Data.Normalize();
            int periodCount = 4;
            double effectiveWindowScale = 1.0 / (1.0 - (Computer.HannWindowCache.EffectiveOffsetRatio * 2.0));
            double percentRatio = 1.0 / type.Count();
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= percentRatio;
            if (type.HasFlag(InstrumentModelType.Sinusoid))
            {
                Sinusoid = new Sinusoid(wave, Computer.NoteToFrequency(midiNote.NoteNumber) / (float)(effectiveWindowScale * periodCount), sinusoidParameters.AreFrequenciesFixed, sinusoidParameters.MinimumDecibelAmplitude.Value, sinusoidParameters.ContinuationRange.Value, sinusoidParameters.MaximumSleepingTime.Value, sinusoidParameters.MinimumLength.Value, token);
            }
            if (type.HasFlag(InstrumentModelType.Transient))
            {
                Transient = new Transient(wave, 25f, 2, transientParameters.Strength.Value, transientParameters.AdjacencyNumber.Value, transientParameters.FlagRatio.Value, transientParameters.IsTransposable, token);
            }
            if (type.HasFlag(InstrumentModelType.Noise))
            {
                Noise = new Noise(wave, (float)(noiseParameters.SamplingFrequency.Value / effectiveWindowScale), token);
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= percentRatio;
        }

        #endregion
    }

    /// <summary>
    /// <see cref="Note.NoteNumber"/> comparer for the <see cref="Note"/> class.
    /// </summary>
    public class NoteNoteNumberComparer : IComparer<Note>
    {
        public int Compare(Note x, Note y)
        {
            return x.NoteNumber.CompareTo(y.NoteNumber);
        }
    }

}
