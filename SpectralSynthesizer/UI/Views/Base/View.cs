using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer.UI
{
    /// <summary>
    /// The base class for every kind of view.
    /// </summary>
    public class View : UserControl
    {

        #region Methods

        /// <summary>
        /// Called when setting the <see cref="DataContextProperty"/>.
        /// Notifies the given instance that the <see cref="DataContext"/> has changed.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <param name="e">Event args.</param>
        private static void OnDataContextChangedCallback(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var view = (source as View);
            if (e.NewValue != e.OldValue && e.NewValue != null)
            {
                view.OnDataContextChanged();
            }
        }

        /// <summary>
        /// Should be called once when the datacontext is given to this control.
        /// </summary>
        protected virtual void OnDataContextChanged()
        {
            var dc = DataContext as ViewViewModel;
            this.MouseDown += (a, b) => dc.OnMiddleMouseClicked();
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Static constructor for hooking up to the <see cref="UserControl.DataContext"/>'s property changed event
        /// </summary>
        static View()
        {
            UserControl.DataContextProperty.OverrideMetadata(typeof(View), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDataContextChangedCallback)));
        }

        #endregion
    }
}
