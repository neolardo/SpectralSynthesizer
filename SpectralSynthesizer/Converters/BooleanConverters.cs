using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace SpectralSynthesizer
{
    #region Reverser

    /// <summary>
    /// Reverses a boolean.
    /// </summary>
    public class BooleanReverserConverter : BaseValueConverter<BooleanReverserConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    #endregion

    #region Null

    /// <summary>
    /// Converts a null value to false.
    /// </summary>
    public class NullToBooleanConverter : BaseValueConverter<NullToBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                if (parameter == null)
                    return false;
                else
                    return true;
            }
            else
            {
                if (parameter == null)
                    return true;
                else
                    return false;
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    #endregion

    #region Color

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.Theme"/> and a false value to <see cref="ApplicationColor.BackgroundDark"/>.
    /// </summary>
    public class BooleanToBackgroundColorConverter : BaseValueConverter<BooleanToBackgroundColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundDark);
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.ForegroundLight"/> and a false value to <see cref="ApplicationColor.BackgroundLight"/>.
    /// </summary>
    public class BooleanToSingleMidiColorConverter : BaseValueConverter<BooleanToSingleMidiColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundLight) : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundLight);
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.Theme"/> and a false value to <see cref="ApplicationColor.ForegroundDark"/>.
    /// </summary>
    public class BooleanToSelectionBoxColorConverter : BaseValueConverter<BooleanToSelectionBoxColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
            return (bool)value ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Cursor

    /// <summary>
    /// Converts a true value to a <see cref="Cursors.IBeam"/> and a false value to <see cref="Cursors.Arrow"/>.
    /// </summary>
    public class BooleanToIBeamCursorConverter : BaseValueConverter<BooleanToIBeamCursorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Cursors.IBeam : Cursors.Arrow;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to a <see cref="Cursors.None"/> and a false value to <see cref="Cursors.Arrow"/>.
    /// </summary>
    public class BooleanToNoneCursorConverter : BaseValueConverter<BooleanToNoneCursorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Cursors.None : Cursors.Arrow;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to a <see cref="Cursors.SizeWE"/> and a false value to <see cref="Cursors.Arrow"/>.
    /// </summary>
    public class BooleanToVerticalResizeCursorConverter : BaseValueConverter<BooleanToVerticalResizeCursorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Cursors.SizeWE : Cursors.Arrow;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Visibility

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.Visible"/> and a false value to <see cref="ApplicationColor.Collapsed"/>.
    /// </summary>
    public class BooleanToCollapsedVisibilityConverter : BaseValueConverter<BooleanToCollapsedVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            else
                return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.Visible"/> and a false value to <see cref="ApplicationColor.Hidden"/>.
    /// </summary>
    public class BooleanToHiddenVisibilityConverter : BaseValueConverter<BooleanToHiddenVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return (bool)value ? Visibility.Visible : Visibility.Hidden;
            else
                return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    #endregion

    #region Alignment

    /// <summary>
    /// Converts a true value to <see cref="HorizontalAlignment.Left"/>, and a false value to <see cref="HorizontalAlignment.Right"/>.
    /// </summary>
    public class BooleanToHorizontalAlignmentConverter : BaseValueConverter<BooleanToHorizontalAlignmentConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? HorizontalAlignment.Left : HorizontalAlignment.Right;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a true value to <see cref="VerticalAlignment.Top"/>, and a false value to <see cref="VerticalAlignment.Bottom"/>.
    /// </summary>
    public class BooleanToVerticalAlignmentConverter : BaseValueConverter<BooleanToVerticalAlignmentConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? VerticalAlignment.Top : VerticalAlignment.Bottom;
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Enums

    
    /// <summary>
    /// Returns true if the <see cref="MidiViewCursorType"/> matches with the string parameter.
    /// </summary>
    public class MatchingMidiViewCursorTypeToBooleanConverter : BaseValueConverter<MatchingMidiViewCursorTypeToBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || parameter == null)
                return false;
            return parameter.ToString().ToLower() == ((MidiViewCursorType)value).ToString().ToLower();
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return null;
            switch (parameter.ToString().ToLower())
            {
                case "selection":
                    return (bool)value ? MidiViewCursorType.Selection : MidiViewCursorType.Adjustment;
                case "adjustment":
                    return (bool)value ? MidiViewCursorType.Adjustment : MidiViewCursorType.Selection;
                default:
                    throw new Exception("Invalid MidiViewCursorType value.");
            }
        }
    }

    /// <summary>
    /// Returns true if the <see cref="SpectogramViewCursorType"/> matches with the string parameter.
    /// </summary>
    public class MatchingSpectogramViewCursorTypeToBooleanConverter : BaseValueConverter<MatchingSpectogramViewCursorTypeToBooleanConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || parameter == null)
                return false;
            return parameter.ToString().ToLower() == ((SpectogramViewCursorType)value).ToString().ToLower();
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return null;
            switch (parameter.ToString().ToLower())
            {
                case "selection":
                    return (bool)value ? SpectogramViewCursorType.Selection : SpectogramViewCursorType.Spectrum;
                case "spectrum":
                    return (bool)value ? SpectogramViewCursorType.Spectrum : SpectogramViewCursorType.Selection;
                default:
                    throw new Exception("Invalid SpectogramViewCursorType value.");
            }
        }
    }

    #endregion
}