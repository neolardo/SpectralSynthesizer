using NAudio.Midi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents a MIDI.
    /// </summary>
    public class Midi : TimeDomainModel, IRenderable
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The location of the MIDI file.
        /// </summary>
        public string FilePath { get; private set; } = "";

        [JsonIgnore]
        /// <summary>
        /// The name of the MIDI file.
        /// </summary>
        public string FileName
        {
            get
            {
                if (FilePath.Contains('\\'))
                    return FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
                else if (FilePath.Contains('/'))
                    return FilePath.Substring(FilePath.LastIndexOf('/') + 1);
                else
                    return FilePath;
            }
        }

        /// <summary>
        /// The <see cref="MidiNote"/> list.
        /// </summary>
        public List<MidiNote> Notes { get; set; } = new List<MidiNote>();

        #endregion

        #region Methods

        #region Creation

        /// <summary>
        /// Gets the <see cref="MidiNote"/>s from the <see cref="Wave"/> data with the correct position and amplitude values. This function can be cancelled.
        /// </summary>
        /// <param name="waveData">The <see cref="Wave"/> data.</param>
        /// <param name="samplerate">The samplerate of the <see cref="Wave"/>.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any possible <see cref="MidiNote"/> to be noticed.</param>
        /// <param name="lengthMinimum">The minimum length of any possible note in milliseconds.</param>
        /// <param name="silenceMaximum">The maximum amount of silence in any possible note in milliseconds.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        private void GetMidiNotesFromWaveData(float[] waveData, int samplerate, float minimumDecibel, int lengthMinimum, int silenceMaximum, CancellationToken token)
        {
            int i = 0;
            lengthMinimum = (int)(lengthMinimum / 1000.0 * samplerate);
            silenceMaximum = (int)(silenceMaximum / 1000.0 * samplerate);
            int length = 0;
            int silence = 0;
            float amp = 0;
            float minimumPCMAmplitude = Computer.DecibelToPCMAmplitude(minimumDecibel);
            while (i < waveData.Length)
            {
                if (Math.Abs(waveData[i]) > minimumPCMAmplitude)
                {
                    if (silence > silenceMaximum)
                    {
                        token.ThrowIfCancellationRequested();
                        if (length > lengthMinimum)
                        {
                            Notes.Add(new MidiNote(0, amp, (int)(((float)(i - length - silence) / samplerate) * 1000f), (int)(((float)length / samplerate) * 1000f)));
                        }
                        length = 0;
                        silence = 0;
                        amp = 0;
                    }
                    else
                    {
                        length += silence;
                        silence = 0;
                    }
                    int j = i;
                    while (j + 1 < waveData.Length && Math.Abs(waveData[j]) > minimumPCMAmplitude)
                    {
                        // go up the hill
                        while (j + 1 < waveData.Length && Math.Abs(waveData[j]) < Math.Abs(waveData[j + 1]))
                            j++;
                        if (Math.Abs(waveData[i]) > amp)
                            amp = Math.Abs(waveData[i]);
                        // go down the hill
                        while (j + 1 < waveData.Length && Math.Abs(waveData[j]) >= Math.Abs(waveData[j + 1]))
                            j++;
                    }
                    length += j - i + 1;
                    i = j;
                }
                else
                {
                    silence += 1;
                }
                i++;
            }
            //last note
            if (length > lengthMinimum)
            {
                Notes.Add(new MidiNote(0, amp, (int)(((float)(i - length - silence) / samplerate) * 1000f), (int)(((float)length / samplerate) * 1000f)));
            }
        }

        /// <summary>
        /// Sets the <see cref="MidiNote.NoteNumber"/>s for the <see cref="MidiNote"/>s in this <see cref="Midi"/>, based on the given <see cref="Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        private void SetNoteNumbersFromWaveData(Wave wave, CancellationToken token)
        {
            for (int i = 0; i < Notes.Count; i++)
            {
                int noteNumber = Computer.FrequencyToNote(new Spectrum(wave.GetSelectedData(Notes[i].Start, Notes[i].Start + Notes[i].Length), wave.WaveFormat.SampleRate * wave.WaveFormat.Channels, ProjectModel.GlobalMinimumDecibel, WindowingType.Full, token).FundamentalSpectral.Frequency);
                Notes[i] = new MidiNote(noteNumber, Notes[i].Amplitude, Notes[i].Start, Notes[i].Length);
                token.ThrowIfCancellationRequested();
            }
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders this <see cref="Midi"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            var buffer = new float[length];
            foreach (var note in Notes)
            {
                int noteStart = (int)(note.Start * (sampleRate / 1000f));
                var sineWave = note.Render(sampleRate, length, token);
                for (int i = 0; i < sineWave.Length; i++)
                {
                    buffer[noteStart + i] += sineWave[i];
                }
            }
            return buffer;
        }

        #endregion

        /// <summary>
        /// Makes this MIDI into a single MIDI so that only one note can be played at a time.
        /// </summary>
        /// <param name="length">The goal length of the MIDI.</param>
        public void MakeToSingle(int length)
        {
            Notes.Sort(new MidiNoteStartComparer());
            for (int i = 1; i < Notes.Count; i++)
            {
                if (Notes[i - 1].Start + Notes[i - 1].Length > Notes[i].Start || Notes[i].Start >= length) // delete if the overlaps or out of the length
                {
                    Notes.RemoveAt(i);
                    i -= 1;
                }
                else if (Notes[i].Start < length && Notes[i].Start + Notes[i].Length >= length) // trim if too long
                {
                    Notes[i] = new MidiNote(Notes[i].NoteNumber, Notes[i].Amplitude, Notes[i].Start, Notes[i].Length - Notes[i].Start);
                }
            }
            Length = length;
        }

        /// <summary>
        /// Delays this <see cref="Midi"/> with the given milliseconds.
        /// </summary>
        /// <param name="delay">The amount of delay in milliseconds.</param>
        public void Delay(int delay)
        {
            Notes = (from note in Notes
                     select new MidiNote(note.NoteNumber, note.Amplitude, note.Start + delay, note.Length)).ToList();
            Length += delay;
        }

        /// <summary>
        /// Gets the a list of <see cref="MidiNote"/>s from this <see cref="Midi"/> inside a given selection range.
        /// </summary>
        /// <param name="selection">The start and end ratios of the <see cref="Midi"/>'s length indicating the selected part.</param>
        /// <param name="isInclusive">The selection is inclusive if the selected notes are not cropped at the selection end and start. By default they are cropped.</param>
        /// <returns>The list of <see cref="MidiNote"/>s from this <see cref="Midi"/> inside the given selection range.</returns>
        public List<MidiNote> GetSelectedNotes((double start, double end) selection, bool isInclusive = false)
        {
            var selectedNotes = new List<MidiNote>();
            int selectionStart = (int)(Length * selection.start);
            int selectionEnd = (int)(Length * selection.end);
            if (isInclusive)
            {
                selectedNotes = (from note in Notes
                                 where (note.Start + note.Length) > selectionStart && note.Start < selectionEnd
                                 select note).ToList();
            }
            else
            {
                foreach (var note in Notes)
                {
                    if ((note.Start + note.Length) > selectionStart && note.Start < selectionEnd)
                    {
                        int noteEnd = Computer.ClampMax(note.Start + note.Length, selectionEnd);
                        int noteStart = Computer.ClampMin(note.Start, selectionStart);
                        int noteLength = (noteEnd - noteStart);
                        var newMidiNote = new MidiNote(note.NoteNumber, note.Amplitude, noteStart, noteLength);
                        selectedNotes.Add(newMidiNote);
                    }
                }
            }
            return selectedNotes;
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Midi"/> class from the given MIDI file's path.
        /// </summary>
        ///  <param name="filePath">The full path to the MIDI file.</param>
        public Midi(string filePath)
        {
            FilePath = filePath;
            var mf = new MidiFile(filePath, false);
            for (int trackInd = 0; trackInd < mf.Tracks; trackInd++)
            {
                // assuming there is only one tempo event per track
                var tempoEvent = mf.Events[trackInd].OfType<TempoEvent>().FirstOrDefault();
                if (tempoEvent == null)
                {
                    // by default the tempo is 120 BMP
                    tempoEvent = new TempoEvent(500000, 48);
                }
                foreach (var midiEvent in mf.Events[trackInd])
                {
                    if (MidiEvent.IsNoteOn(midiEvent))
                    {
                        var noteOn = midiEvent as NoteOnEvent;
                        var midiNote = new MidiNote(noteOn.NoteNumber, noteOn.Velocity / 127f, (int)(noteOn.AbsoluteTime / (double)mf.DeltaTicksPerQuarterNote * tempoEvent.MicrosecondsPerQuarterNote / 1000.0), (int)(noteOn.NoteLength / (double)mf.DeltaTicksPerQuarterNote * tempoEvent.MicrosecondsPerQuarterNote / 1000.0));
                        Notes.Add(midiNote);
                        if (midiNote.Start + midiNote.Length > Length)
                        {
                            Length = midiNote.Start + midiNote.Length;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Midi"/> class from a given <see cref="Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/> input.</param>
        /// <param name="selection">The start and end ratios of the <see cref="Wave"/>'s length indicating the relevant part of the conversion.</param>
        /// <param name="minimumDecibel">The minimum decibel amplitude of any possible note to be noticed.</param>
        /// <param name="minimumLength">The minimum length of any possible note in milliseconds.</param>
        /// <param name="maximumSilence">The maximum amount of silence in any possible note in milliseconds.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        public Midi(Wave wave, (double start, double end) selection, float minimumDecibel, int minimumLength, int maximumSilence, CancellationToken token)
        {
            int startIndex = (int)(wave.Data.Length * selection.start);
            int waveLength = (int)(wave.Data.Length * (selection.end - selection.start));
            float[] relevantData = new float[waveLength];
            Array.Copy(wave.Data, startIndex, relevantData, 0, waveLength);
            GetMidiNotesFromWaveData(relevantData, wave.WaveFormat.SampleRate * wave.WaveFormat.Channels, minimumDecibel, minimumLength, maximumSilence, token);
            Delay((int)(wave.Length * selection.start));
            Length = wave.Length;
            SetNoteNumbersFromWaveData(wave, token);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Midi"/> class.
        /// </summary>
        /// <param name="length">The length of this <see cref="Midi"/> in milliseconds.</param>
        public Midi(int length) { Length = length; }

        /// <summary>
        /// Initializes an empty instance of the <see cref="Midi"/> class.
        /// </summary>
        [JsonConstructor]
        public Midi() { }

        #endregion
    }
}
