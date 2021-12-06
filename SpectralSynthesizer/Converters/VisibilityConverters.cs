using System;
using System.Globalization;
using System.Windows;

namespace SpectralSynthesizer
{
    #region Null

    /// <summary>
    /// Convers the given object to <see cref="Visibility.Hidden"/> if there is no parameter and the value is null.
    /// </summary>
    public class NullToHiddenVisibilityConverter : BaseValueConverter<NullToHiddenVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter == null)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
            else
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convers the given object to <see cref="Visibility.Collapsed"/> if there is no parameter and the value is null.
    /// </summary>
    public class NullToCollapsedVisibilityConverter : BaseValueConverter<NullToCollapsedVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter == null)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            else
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a null or empty string to <see cref="Visibility.Collapsed"/> if there is no parameter.
    /// </summary>
    public class NullOrEmptyStringToCollapsedVisibilityConverter : BaseValueConverter<NullOrEmptyStringToCollapsedVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            string str = value.ToString();
            if (String.IsNullOrEmpty(str))
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
            {
                if (parameter == null)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a null or empty string to <see cref="Visibility.Hidden"/> if there is no parameter.
    /// </summary>
    public class NullOrEmptyStringToHiddenVisibilityConverter : BaseValueConverter<NullOrEmptyStringToHiddenVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            string str = value.ToString();
            if (String.IsNullOrEmpty(str))
            {
                if (parameter == null)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            else
            {
                if (parameter == null)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion

    #region Enums

    /// <summary>
    /// Converts an <see cref="ApplicationPage"/> to <see cref="Visibility.Visible"/> if the parameter is the same as the value, otherwise to <see cref="Visibility.Hidden"/>.
    /// </summary>
    public class WaveToMidiConversionStatusToHiddenVisibilityConverter : BaseValueConverter<WaveToMidiConversionStatusToHiddenVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (LoadingStatus)value;
            if (parameter == null)
            {
                if (status == LoadingStatus.Loaded)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            else
            {
                if (status == LoadingStatus.Loaded)
                    return Visibility.Hidden;
                else
                    return Visibility.Visible;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an <see cref="ApplicationPage"/> to <see cref="Visibility.Visible"/> if the parameter is the same as the value, otherwise to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class ApplicationPageToVisibilityConverter : BaseValueConverter<ApplicationPageToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ApplicationPage page = (ApplicationPage)value;
            if (page == ApplicationPage.InstrumentBuilder && parameter.ToString() == "instrument")
                return Visibility.Visible;
            if (page == ApplicationPage.AudioRenderer && parameter.ToString() == "render")
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an <see cref="ExportationType"/> to <see cref="Visibility.Visible"/> if the parameter is the same as the value, otherwise to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class MatchingExportationTypeToVisibilityConverter : BaseValueConverter<MatchingExportationTypeToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;
            return (parameter.ToString() == ((ExportationType)value).ToString().ToLower())
               ? Visibility.Visible
               : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts an <see cref="PreferenceMenuItem"/> to <see cref="Visibility.Visible"/> if the parameter is the same as the value, otherwise to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class MatchingPreferenceMenuToVisibilityConverter : BaseValueConverter<MatchingPreferenceMenuToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return Visibility.Collapsed;
            return (parameter.ToString() == ((PreferenceMenuItem)value).ToString().ToLower())
               ? Visibility.Visible
               : Visibility.Collapsed;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region Matching Row Numbers

    /// <summary>
    /// Converts mathing integers to <see cref="Visibility.Visible"/>, otherwise to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public class MatchingRowToVisibilityConverter : BaseValueConverter<MatchingRowToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;
            if ((int)(value) == Int32.Parse(parameter.ToString()))
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Highlight

    /// <summary>
    /// Converts a <see cref="ApplicationColor.ForegroundLight"/> value to <see cref="Visibility.Visible"/> otherwise to <see cref="Visibility.Collapsed"/>. 
    /// </summary>
    public class HighlightToHiddenVisibilityConverter : BaseValueConverter<HighlightToHiddenVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            if ((ApplicationColor)value == ApplicationColor.ForegroundLight)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
