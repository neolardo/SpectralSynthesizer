using PropertyChanged;
using System.ComponentModel;

namespace SpectralSynthesizer.Models
{
    /// <summary>
    /// A base model which fires the PropertyChanged event when something changes.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public abstract class BaseModel : INotifyPropertyChanged
    {
        #region Delegates and Events

        /// <summary>
        /// An events which fires when any child property changes it's value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        #endregion

        #region Methods

        /// <summary>
        /// Call this to fire a new <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Call this to fire a new <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event arguments.</param>
        public void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(""));
        }

        /// <summary>
        /// Gets a deep copy of this model.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public abstract BaseModel GetDeepCopy();

        #endregion
    }
}
