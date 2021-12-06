using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// A byte storage which buffers the necessary part of data at all times.
    /// First the selected part will be filled, and later the remaning part until the buffer is completely full.
    /// All data is stored in bytes but it is managed as floats, in 4 byte chunks.
    /// </summary>
    public class ImperativeBuffer : BaseModel
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing off events when this buffer has been modified.
        /// </summary>
        public delegate void BufferModifiedDelegate();

        /// <summary>
        /// Fires off when this buffer has been modified.
        /// </summary>
        public event BufferModifiedDelegate BufferModified;

        #endregion

        #region Properties

        [JsonProperty]
        /// <summary>
        /// The byte array data of this buffer.
        /// </summary>
        protected byte[] Data { get; private set; }

        [JsonProperty]
        /// <summary>
        /// The list of filled and unfilled sections of this buffer which helps to navigate in the buffer. The start and end indexes of the sections are measured in floats.
        /// </summary>
        public List<ImperativeBufferSection> Sections { get; private set; } = new List<ImperativeBufferSection>();

        [JsonProperty]
        /// <summary>
        /// The selected part of the buffer. The indexes range from 0 to the length of the data in floats.
        /// </summary>
        public (int Start, int End) Selection { get; private set; } = (0, 0);

        [JsonIgnore]
        /// <summary>
        /// True if the buffering has completed, otherwise false.
        /// </summary>
        public bool IsBufferingComplete => Sections.Count == 1 && Sections[0].IsFilled;

        [JsonIgnore]
        /// <summary>
        /// A lock used whenever the <see cref="Data"/> is used in any way.
        /// </summary>
        protected object DataLock => new object();

        [JsonIgnore]
        /// <summary>
        /// A lock used whenever the <see cref="Sections"/> are used in any way.
        /// </summary>
        protected object SectionLock => new object();

        [JsonIgnore]
        /// <summary>
        /// The length of the data array in floats.
        /// </summary>
        public int Length => Data == null ? 0 : Data.Length / 4;


        #endregion

        #region Methods

        #region Data Insertion

        /// <summary>
        /// Inserts new float array data to the buffer.
        /// </summary>
        /// <param name="newData">The new float array data to insert.</param>
        /// <param name="startIndex">The destination start index of the data inside the whole buffer measured in floats.</param>
        public void InsertData(float[] newData, int startIndex)
        {
            lock (DataLock)
            {
                Buffer.BlockCopy(newData, 0, Data, startIndex * 4, newData.Length * 4);
            }
            InsertSection(startIndex, newData.Length);
        }

        /// <summary>
        /// Inserts new byte array data to the buffer.
        /// </summary>
        /// <param name="newData">The new byte array data to insert.</param>
        /// <param name="startIndex">The destination start index of the data inside the whole buffer measured in bytes.</param>
        public void InsertData(byte[] newData, int startIndex)
        {
            lock (DataLock)
            {
                Array.Copy(newData, 0, Data, startIndex, newData.Length);
            }
            InsertSection(startIndex / 4, newData.Length / 4);
        }

        /// <summary>
        /// Inserts a new filled <see cref="ImperativeBufferSection"/> to the <see cref="Sections"/> list.
        /// </summary>
        /// <param name="start">The start index of the new section.</param>
        /// <param name="length">The length of the new section.</param>
        private void InsertSection(int start, int length)
        {
            var startSection = GetSection(start);
            var endSection = GetSection(start + length - 1);
            if (startSection == endSection)
            {
                if (startSection.IsFilled)
                {
                    return;
                }
                else
                {
                    InsertSectionBetween(startSection, start, length);
                    CleanUpSections();
                }
            }
            else
            {
                InsertSectionBetween(startSection, endSection, start, length);
                CleanUpSections();
            }
        }

        /// <summary>
        /// Inserts a new <see cref="ImperativeBufferSection"/> inside an existing one.
        /// </summary>
        /// <param name="section">The section in where the new section will be inserted to.</param>
        /// <param name="start">The start of the new section.</param>
        /// <param name="length">The length of the new section.</param>
        private void InsertSectionBetween(ImperativeBufferSection section, int start, int length)
        {
            lock (SectionLock)
            {
                var sectionBefore = new ImperativeBufferSection(section.Start, start - section.Start, section.IsFilled);
                var sectionBetween = new ImperativeBufferSection(start, length, true);
                var sectionAfter = new ImperativeBufferSection(start + length, section.Start + section.Length - (start + length), section.IsFilled);
                int ind = Sections.IndexOf(section);
                Sections.RemoveAt(ind);
                if (sectionAfter.Length > 0)
                {
                    Sections.Insert(ind, sectionAfter);
                }
                if (sectionBetween.Length > 0)
                {
                    Sections.Insert(ind, sectionBetween);
                }
                if (sectionBefore.Length > 0)
                {
                    Sections.Insert(ind, sectionBefore);
                }
            }
        }

        /// <summary>
        /// Inserts a new <see cref="ImperativeBufferSection"/> between two sections.
        /// </summary>
        /// <param name="startSection">The section to the left side of the insertable section.</param>
        /// <param name="endSection">The section to the right side of the insertable section.</param>
        /// <param name="start">The start of the new section.</param>
        /// <param name="length">The length of the new section.</param>
        private void InsertSectionBetween(ImperativeBufferSection startSection, ImperativeBufferSection endSection, int start, int length)
        {
            lock (SectionLock)
            {
                int startSectionInd = Sections.IndexOf(startSection);
                int endSectionInd = Sections.IndexOf(endSection);
                for (int i = startSectionInd; i <= endSectionInd; i++)
                {
                    Sections.RemoveAt(i);
                    i -= 1;
                    endSectionInd -= 1;
                }
                var sectionBefore = new ImperativeBufferSection(startSection.Start, start - startSection.Start, startSection.IsFilled);
                var sectionBetween = new ImperativeBufferSection(start, length, true);
                var sectionAfter = new ImperativeBufferSection(start + length, endSection.Start + endSection.Length - (start + length), endSection.IsFilled);
                if (sectionAfter.Length > 0)
                {
                    Sections.Insert(startSectionInd, sectionAfter);
                }
                if (sectionBetween.Length > 0)
                {
                    Sections.Insert(startSectionInd, sectionBetween);
                }
                if (sectionBefore.Length > 0)
                {
                    Sections.Insert(startSectionInd, sectionBefore);
                }
            }
        }

        /// <summary>
        /// Combines <see cref="ImperativeBufferSection"/>s next to each other if both of them are filled or unfilled.
        /// </summary>
        private void CleanUpSections()
        {
            lock (SectionLock)
            {
                for (int i = 0; i < Sections.Count - 1; i++)
                {
                    if (Sections[i].IsFilled == Sections[i + 1].IsFilled)
                    {
                        var combinedSection = new ImperativeBufferSection(Sections[i].Start, Sections[i].Length + Sections[i + 1].Length, Sections[i].IsFilled);
                        Sections.RemoveAt(i);
                        Sections.RemoveAt(i);
                        Sections.Insert(i, combinedSection);
                        i -= 1;
                    }
                }
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// Changes the selected part of this buffer.
        /// </summary>
        /// <param name="startRatio">The start ratio of the newly selected part.</param>
        /// <param name="endRatio">The end ratio of the newly selected part.</param>
        public virtual void ChangeSelection(double startRatio, double endRatio)
        {
            lock (DataLock)
            {
                Selection = ((int)(startRatio * (Data.Length / 4)), (int)(endRatio * (Data.Length / 4)));
                BufferModified?.Invoke();
            }
        }

        #endregion

        #region Sections

        /// <summary>
        /// Gets the <see cref="ImperativeBufferSection"/> from the <see cref="Sections"/> list corresponding to the given index of float data.
        /// </summary>
        /// <param name="index">The index of the data where the section is searched, measured in floats.</param>
        /// <returns>The <see cref="ImperativeBufferSection"/> from the <see cref="Sections"/> list corresponding to the given index of data.</returns>
        protected ImperativeBufferSection GetSection(int index)
        {
            lock (SectionLock)
            {
                foreach (var sec in Sections)
                {
                    if (sec.Start <= index && index < sec.Start + sec.Length)
                        return sec;
                }
                throw new Exception("Buffer section not found at the specified index.");
            }
        }

        /// <summary>
        /// Gets the next empty <see cref="ImperativeBufferSection"/> of this buffer.
        /// </summary>
        /// <param name="bufferLength">The maximum length of the next empty section in floats.</param>
        /// <returns>The next empty <see cref="ImperativeBufferSection"/> of this buffer.</returns>
        public ImperativeBufferSection GetNextEmptySection(int bufferLength)
        {
            lock (SectionLock)
            {
                var section = GetSection(Selection.Start);
                if (section.IsFilled)
                {
                    if (Sections.IndexOf(section) + 1 < Sections.Count)
                    {
                        section = Sections[Sections.IndexOf(section) + 1];
                    }
                    else
                    {
                        int index = 0;
                        while (index < Sections.Count && Sections[index].IsFilled)
                        {
                            index++;
                        }
                        if (index < Sections.Count)
                        {
                            section = Sections[index];
                        }
                        else
                        {
                            throw new Exception("No empty section is found, buffer is completely filled.");
                        }
                    }
                }
                return new ImperativeBufferSection(section.Start, Math.Min(bufferLength, section.Length), false);
            }
        }

        #endregion

        #region Init

        /// <summary>
        /// Initializes this <see cref="ImperativeBuffer"/>.
        /// </summary>
        /// <param name="length">The length of this buffer in floats.</param>
        protected virtual void Init(int length)
        {
            lock (DataLock)
            {
                lock (SectionLock)
                {
                    Data = new byte[length * 4];
                    Sections.Clear();
                    Sections.Add(new ImperativeBufferSection(0, length, false));
                    Selection = (0, length);
                    BufferModified?.Invoke();
                }
            }
        }

        #endregion

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new ImperativeBuffer(Data.Length)
            {
                Data = (byte[])this.Data.Clone(),
                Sections = new List<ImperativeBufferSection>(this.Sections),
                Selection = this.Selection
            };
        }
        #endregion

        #region Constructor

        /// <summary>
        /// Inizializes a new instance of the <see cref="ImperativeBuffer"/> class.
        /// </summary>
        /// <param name="floatNumber">The length of this buffer in floats.</param>
        public ImperativeBuffer(int floatNumber)
        {
            Init(floatNumber);
        }

        [JsonConstructor]
        /// <summary>
        /// Initializes an empty <see cref="ImperativeBuffer"/>. <see cref="Init"/> should be called before using the buffer.
        /// </summary>
        public ImperativeBuffer()
        {
            Data = new byte[0];
        }

        #endregion
    }
}
