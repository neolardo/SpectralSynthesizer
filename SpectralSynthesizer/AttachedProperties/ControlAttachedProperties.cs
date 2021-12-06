using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SpectralSynthesizer
{
    /// <summary>
    /// An attached property to write descriptions
    /// </summary>
    public class DescriptionProperty : BaseAttachedProperty<DescriptionProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }


    /// <summary>
    /// An attached property to play music
    /// </summary>
    public class PlayerProperty : BaseAttachedProperty<PlayerProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
