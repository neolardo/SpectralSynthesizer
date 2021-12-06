using System;
using System.Globalization;

namespace SpectralSynthesizer
{
    #region Loading Window

    /// <summary>
    /// Converts the loading percent to the loading bar width.
    /// </summary>
    public class LoadingPercentToBorderWidthConverter : BaseValueConverter<LoadingPercentToBorderWidthConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0;
            return (double)value * Double.Parse(parameter.ToString());
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Slider

    /// <summary>
    /// Modifies a slider's value.
    /// </summary>
    public class SliderValueConverter : BaseValueConverter<SliderValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0;
            double doubleValue = (double)value;
            switch (parameter as string)
            {
                case "cube":
                    return Math.Pow(doubleValue, 1.0 / 3.0);
                case "square":
                    return Math.Sqrt(doubleValue);
                default:
                    return doubleValue;

            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0;
            double doubleValue = (double)value;
            switch (parameter as string)
            {
                case "cube":
                    return doubleValue * doubleValue * doubleValue;
                case "square":
                    return doubleValue * doubleValue;
                default:
                    return doubleValue;
            }
        }
    }

    #endregion
}
