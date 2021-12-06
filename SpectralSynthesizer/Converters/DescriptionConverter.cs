using System;
using System.Globalization;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Shows a description based on the parameter.
    /// </summary>
    public class DescriptionConverter : BaseValueConverter<DescriptionConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            if ((bool)value)
            {
                IoC.Get<ProjectViewModel>().ShowDescription(parameter.ToString());
            }
            return false;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
