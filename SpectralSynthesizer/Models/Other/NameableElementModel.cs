using SpectralSynthesizer.Models.Interfaces;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Represents a basic implementation of the <see cref="INameable"/> interface as a <see cref="BaseModel"/>.
    /// </summary>
    public class NameableElementModel : BaseModel, INameable
    {
        #region Properties

        /// <summary>
        /// The name of this element.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new NameableElementModel(Name);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NameableElementModel"/> class.
        /// </summary>
        /// <param name="name">The name of this element.</param>
        public NameableElementModel(string name)
        {
            Name = name;
        }

        #endregion
    }

    /// <summary>
    /// Represents a basic implementation of the <see cref="INameable"/> interface as a <see cref="BaseModel"/> with <see cref="T"/> value.
    /// </summary>
    /// <typeparam name="T">The type of the value stored inside this element.</typeparam>
    public class NameableElementModel<T> : BaseModel, INameable
    {
        #region Properties

        /// <summary>
        /// The name of this element.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of this element.
        /// </summary>
        public T Value { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new NameableElementModel(Name);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NameableElementModel"/> class.
        /// </summary>
        /// <param name="value">The value of this element.</param>
        /// <param name="name">The name of this element.</param>
        public NameableElementModel(T value, string name)
        {
            Value = value;
            Name = name;
        }

        #endregion
    }
}
