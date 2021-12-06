using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Represents one unit of the ruler
    /// </summary>
    public class RulerItemViewModel : BaseViewModel
    {
        /// <summary>
        /// The ruler item value in string
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The position of this item.
        /// Can be horionztal and vertical aswell.
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// The size of this ruler item
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RulerItemViewModel(double pos, string text, double size)
        {
            Position = pos;
            Text = text;
            Size = size;
        }
    }
}
