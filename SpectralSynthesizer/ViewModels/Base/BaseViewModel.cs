using PropertyChanged;
using System.ComponentModel;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A base viewmodel which fires PropertyChanged events for us
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// An events which fires when any child property changes it's value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// Call this to fire a new <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="name"></param>
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

    }
}
