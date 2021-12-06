using System;
using System.ComponentModel;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A  base attached property which replaces the vanilla attached property
    /// </summary>
    /// <typeparam name="Parent">The parent class to the base attached property</typeparam>
    /// <typeparam name="Property">The type of this attached property</typeparam>
    public abstract class BaseAttachedProperty<Parent, Property>
        where Parent : new()
    {
        #region Public events

        /// <summary>
        /// Fired when the value changes
        /// </summary>
        public event Action<DependencyObject, DependencyPropertyChangedEventArgs> ValueChanged = (sender, e) => { };

        /// <summary>
        /// Fired when the value changes, even when the calue is the same
        /// </summary>
        public event Action<DependencyObject, object> ValueUpdated = (sender, value) => { };

        #endregion

        #region Public Properties

        /// <summary>
        /// A singleton instance of this parent class
        /// </summary>
        public static Parent Instance { get; private set; } = new Parent();

        #endregion

        #region Attached Property Definitions

        /// <summary>
        /// The attached property for this class
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.RegisterAttached(
            "Value",
            typeof(Property),
            typeof(BaseAttachedProperty<Parent, Property>),
            new UIPropertyMetadata(
                default(Property),
                new PropertyChangedCallback(OnValuePropertyChanged),
                new CoerceValueCallback(OnValuePropertyUpdated)
                ));

        /// <summary>
        /// The call back event when the <see cref="ValueProperty"/>is changed even if it is the same value
        /// </summary>
        /// <param name="d">The UI element that had its porperty updated</param>
        /// <param name="e">The argument for the event</param>
        private static object OnValuePropertyUpdated(DependencyObject d, object value)
        {
            // Call the parent function
            (Instance as BaseAttachedProperty<Parent, Property>)?.OnValueUpdated(d, value);

            // Call event listeners
            (Instance as BaseAttachedProperty<Parent, Property>)?.ValueUpdated(d, value);

            return value;
        }


        /// <summary>
        /// The call back event when the <see cref="ValueProperty"/>is changed
        /// </summary>
        /// <param name="d">The UI element that had its porperty changed</param>
        /// <param name="e">The argument for the event</param>
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Call the parent function
            (Instance as BaseAttachedProperty<Parent, Property>)?.OnValueChanged(d, e);

            // Call event listeners
            (Instance as BaseAttachedProperty<Parent, Property>)?.ValueChanged(d, e);
        }

        /// <summary>
        /// Gets the attached property
        /// </summary>
        /// <param name="d">The element to get the property from</param>
        /// <returns></returns>
        public static Property GetValue(DependencyObject d) => (Property)d.GetValue(ValueProperty);

        /// <summary>
        /// Sets the attached property
        /// </summary>
        /// <param name="d">This element's property will be set</param>
        /// <param name="value">The value to set to the property</param>
        public static void SetValue(DependencyObject d, Property value) => d.SetValue(ValueProperty, value);

        #endregion

        #region Event Methods

        /// <summary>
        /// The method that is called when any attached property od this type is changed
        /// </summary>
        /// <param name="sender">The UI element that the property was changed for</param>
        /// <param name="e">The arguments for the element</param>
        public virtual void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) { }

        /// <summary>
        /// The method that is called when any attached property od this type is changed, even it the value is the same
        /// </summary>
        /// <param name="sender">The UI element that the property was changed for</param>
        /// <param name="e">The value </param>
        public virtual void OnValueUpdated(DependencyObject sender, object value) { }

        #endregion
    }
}
