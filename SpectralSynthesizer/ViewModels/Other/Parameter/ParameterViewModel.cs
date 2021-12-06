using SpectralSynthesizer.Models;
using System;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents the viewmodel for the <see cref="Parameter{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParameterViewModel<T> : BaseViewModel where T : struct, IConvertible
    {
        #region Properties

        /// <summary>
        /// The parameter model.
        /// </summary>
        public Parameter<T> Model { get; private set; }

        /// <summary>
        /// The minimum value of the parameter.
        /// </summary>
        public double Minimum { get; private set; }

        /// <summary>
        /// The maximum value of the parameter.
        /// </summary>
        public double Maximum { get; private set; }

        /// <summary>
        /// The current value of the parameter.
        /// </summary>
        public double Value { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the given model as the parameter.
        /// </summary>
        /// <param name="model">The parameter model.</param>
        public void LoadModel(Parameter<T> model)
        {
            Model = model;
            Maximum = Convert.ToDouble(model.Maximum);
            Minimum = Convert.ToDouble(model.Minimum);
            Value = Convert.ToDouble(model.Value);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterViewModel{T}"/> class.
        /// </summary>
        public ParameterViewModel() { }

        #endregion

    }
}
