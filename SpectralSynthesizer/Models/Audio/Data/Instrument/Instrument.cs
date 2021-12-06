using Newtonsoft.Json;
using SpectralSynthesizer.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents a musical instrument.
    /// </summary>
    public class Instrument : BaseModel, INameable
    {
        #region Properites

        [JsonProperty]
        /// <summary>
        /// The name of this instrument.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The list of generated <see cref="Note"/>s.
        /// </summary>
        public List<Note> Notes { get; set; } = new List<Note>();

        /// <summary>
        /// The type of models this <see cref="Instrument"/> consists of.
        /// </summary>
        public InstrumentModelType ModelType { get; set; } = InstrumentModelType.None;

        #region Static

        /// <summary>
        /// The <see cref="LoadingPercentManager"/> for <see cref="Instrument"/> generation.
        /// </summary>
        public static LoadingPercentManager InstrumentGenerationPercentManager { get; } = new LoadingPercentManager();

        /// <summary>
        /// The maximum number of characters a <see cref="Instrument"/>'s name can contain.
        /// </summary>
        public static int MaximumNameLength => 30;

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Get the <see cref="Note"/> with the given <see cref="Note.NoteNumber"/>. If it does not exists in the <see cref="Notes"/> list a new <see cref="Note"/> is generated from the existing <see cref="Notes"/> by interpolation.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="noteNumber">The <see cref="Note.NoteNumber"/> of the searched <see cref="Note"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns>The generated or found <see cref="Note"/>.</returns>
        public Note GetNoteAt(int noteNumber, CancellationToken token)
        {
            var note = Notes.Find(_ => _.NoteNumber == noteNumber);
            if (note == null && Notes.Count > 0)
            {
                note = GenerateNoteAt(noteNumber, token);
            }
            return note;
        }

        /// <summary>
        /// Generates a new <see cref="Note"/> at the given <see cref="Note.NoteNumber"/> by interpolating between the existing ones.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="noteNumber">The <see cref="Note.NoteNumber"/> of the generated <see cref="Note"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns></returns>
        private Note GenerateNoteAt(int noteNumber, CancellationToken token)
        {
            var orderedNotes = new List<Note>(Notes);
            orderedNotes.Sort(new NoteNoteNumberComparer());
            int aIndex = 0;
            int bIndex = Notes.Count - 1;
            while (aIndex < Notes.Count && orderedNotes[aIndex].NoteNumber < noteNumber)
            {
                aIndex++;
            }
            while (bIndex >= 0 && orderedNotes[bIndex].NoteNumber > noteNumber)
            {
                bIndex--;
            }
            return new Note(noteNumber, orderedNotes[Math.Clamp(aIndex, 0, Notes.Count - 1)], orderedNotes[Math.Clamp(bIndex, 0, Notes.Count - 1)], token);
        }

        /// <summary>
        /// Adds or overwrites a <see cref="Note"/> of the <see cref="Notes"/> list.
        /// </summary>
        /// <param name="note">The note to add.</param>
        /// <param name="overwrite">True if the new note whould overwrite the possible existing one.</param>
        public void AddNote(Note note, bool overwrite = false)
        {
            if (!overwrite && !Notes.Exists(n => n.NoteNumber == note.NoteNumber))
            {
                Notes.Add(note);
            }
            else if (overwrite)
            {
                var oldNote = Notes.Find(_ => _.NoteNumber == note.NoteNumber);
                if (oldNote != null)
                {
                    Notes.Remove(oldNote);
                }
                Notes.Add(note);
            }
        }

        /// <summary>
        /// Clears this <see cref="Instrument"/>.
        /// </summary>
        public void Clear()
        {
            this.Notes.Clear();
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            Instrument instrument = new Instrument(this.Name);
            foreach (var note in this.Notes)
                instrument.AddNote((Note)note.GetDeepCopy());
            return instrument;
        }

        /// <summary>
        /// Tries to set the given name to this <see cref="Instrument"/>.
        /// </summary>
        /// <param name="newName">The new name of this <see cref="Instrument"/>.</param>
        /// <returns>The updated name of this <see cref="Instrument"/>.</returns>
        public string TrySetName(string newName)
        {
            if (string.IsNullOrEmpty(newName) == false && string.IsNullOrWhiteSpace(newName) == false && IoC.Get<ProjectModel>().Instruments.FirstOrDefault(_ => _.Name == newName) == null)
            {
                Name = newName;
            }
            return Name;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Instrument"/> class.
        /// </summary> 
        public Instrument() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Instrument"/> class.
        /// </summary>
        /// <param name="name">The name of the instrument.</param>
        public Instrument(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Instrument"/> class from the given <see cref="Wave"/> and <see cref="Midi"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/> with sound of the <see cref="Instrument"/>.</param>
        /// <param name="midi">The <see cref="Midi"/> with the notes aligned to the <see cref="Wave"/>'s notes.</param>.
        /// <param name="selection">The start and end ratios of the <see cref="Wave"/>'s length indicating the relevant part of the generation.</param>
        /// <param name="type">The type of models used in the generation.</param>
        /// <param name="sinusoidParameters">The <see cref="SinusoidParameters"/>.</param>
        /// <param name="transientParameters">The <see cref="TransientParameters"/>.</param>
        /// <param name="noiseParameters">The <see cref="NoiseParameters"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Instrument(Wave wave, Midi midi, (double start, double end) selection, InstrumentModelType type, SinusoidParameters sinusoidParameters, TransientParameters transientParameters, NoiseParameters noiseParameters, CancellationToken token)
        {
            ModelType = type;
            if (type.Count() > 0)
            {
                List<MidiNote> selectedNotes = midi.GetSelectedNotes(selection);
                InstrumentGenerationPercentManager.Init(1.0 / selectedNotes.Count);
                foreach (var midiNote in selectedNotes)
                {
                    var selectedWave = new Wave(wave.GetSelectedData(midiNote.Start, midiNote.Start + midiNote.Length), wave.WaveFormat);
                    AddNote(new Note(selectedWave, midiNote, type, sinusoidParameters, transientParameters, noiseParameters, token));
                }
            }
        }

        #endregion
    }
}
