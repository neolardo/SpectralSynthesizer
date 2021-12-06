using SpectralSynthesizer.Models.Interfaces;
using System.Collections.Generic;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// Manages the history of the application's models with the undo and redo operations.
    /// </summary>
    public static class ModelHistoryManager
    {
        #region Properties

        /// <summary>
        /// The list that contains the previous states of the application. Used when undoing.
        /// </summary>
        private static LinkedList<ModelHistory> PreviousModelStates { get; set; } = new LinkedList<ModelHistory>();

        /// <summary>
        /// The list that contains the future states of the application. Used when redoing.
        /// </summary>
        private static LinkedList<ModelHistory> NextModelStates { get; set; } = new LinkedList<ModelHistory>();

        /// <summary>
        /// Indicates whether undoing is possible or not.
        /// </summary>
        public static bool IsUndoPossible => PreviousModelStates.Count > 1;

        /// <summary>
        /// Indicates whether redoing is possible or not.
        /// </summary>
        public static bool IsRedoPossible => NextModelStates.Count > 0;

        #endregion

        #region Methods

        /// <summary>
        /// Undoes the recent changes made in the application.
        /// </summary>
        public static void Undo()
        {
            if (IsUndoPossible)
            {
                var first = PreviousModelStates.First.Value;
                NextModelStates.AddFirst(new ModelHistory(first.Model.GetDeepCopy(), first.Location));
                PreviousModelStates.RemoveFirst();
                first = PreviousModelStates.First.Value;
                first.Location.LoadModel(first.Model.GetDeepCopy());
            }
        }

        /// <summary>
        /// Redoes the recent changes made in the application.
        /// </summary>
        public static void Redo()
        {
            if (IsRedoPossible)
            {
                var first = NextModelStates.First.Value;
                first.Location.LoadModel(first.Model.GetDeepCopy());
                PreviousModelStates.AddFirst(new ModelHistory(first.Model.GetDeepCopy(), first.Location));
                NextModelStates.RemoveFirst();
            }
        }

        /// <summary>
        /// Saves the current state of the application.
        /// </summary>
        /// <param name="model">The model of which has changed.</param>
        /// <param name="location">The location where the model has changed.</param>
        public static void Save(BaseModel model, IUndoableRedoable<BaseModel> location)
        {
            NextModelStates.Clear();
            PreviousModelStates.AddFirst(new ModelHistory(model, location));
        }

        #endregion

    }
}
