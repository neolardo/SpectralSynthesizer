using System;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The event which is fired when the <see cref="CanExecute(object)"/> value has changed
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Private members

        public Action _action;

        #endregion

        #region Public events

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public RelayCommand(Action action)
        {
            _action = action;
        }

        #endregion

        #region Command methods

        /// <summary>
        /// A relay command can always execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        #endregion
    }

    /// <summary>
    /// The event which is fired when the <see cref="CanExecute(object)"/> value has changed
    /// </summary>
    public class ParameterizedRelayCommand : ICommand
    {
        #region Private members

        public Action<object> _action;

        #endregion

        #region Public events

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ParameterizedRelayCommand(Action<object> action)
        {
            _action = action;
        }

        #endregion

        #region Command methods

        /// <summary>
        /// A relay command can always execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        #endregion
    }
}
