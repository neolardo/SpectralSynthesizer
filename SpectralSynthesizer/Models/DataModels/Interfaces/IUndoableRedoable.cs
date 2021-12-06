
namespace SpectralSynthesizer.Models.Interfaces
{
    /// <summary>
    /// Delegate for fireing events after the model has been saved.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="model">The model.</param>
    public delegate void ModelSavedDelegate<T>(T model);

    /// <summary>
    /// Interface for every class that whom model is undoable and redoable.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    public interface IUndoableRedoable<T>
    {
        /// <summary>
        /// Fires off after the model have been saved.
        /// </summary>
        public event ModelSavedDelegate<T> ModelSaved;

        /// <summary>
        /// The loaded model.
        /// </summary>
        public T Model { get; set; }

        /// <summary>
        /// Loads the model.
        /// </summary>
        /// <param name="model">The model.</param>.
        public void LoadModel(T model);

    }
}
