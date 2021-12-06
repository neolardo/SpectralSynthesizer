using SpectralSynthesizer.Models.Interfaces;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Contains the changed <see cref="BaseModel"/> and the <see cref="IUndoableRedoable{T}"/> place where the change has occured.
    /// </summary>
    public class ModelHistory
    {

        #region Properties

        /// <summary>
        /// The model which have changed.
        /// </summary>
        public BaseModel Model { get; set; }

        /// <summary>
        /// The class where the change has occured.
        /// </summary>
        public IUndoableRedoable<BaseModel> Location { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelHistory"/> class.
        /// </summary>
        public ModelHistory(BaseModel model, IUndoableRedoable<BaseModel> location)
        {
            Model = model;
            Location = location;
        }

        #endregion

    }
}
