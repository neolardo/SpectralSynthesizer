using SpectralSynthesizer.Models;
using SpectralSynthesizer.Models.DataModels.Enums;
using System;
using System.Globalization;

namespace SpectralSynthesizer
{
    #region Null

    /// <summary>
    /// Convers the given object to <see cref="ApplicationColor.ForegroundDark"/> if it is null, otherwise to <see cref="ApplicationColor.Theme"/>.
    /// </summary>
    public class NullToSelectionColorConverter : BaseValueConverter<NullToSelectionColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
            }
            else
            {
                return ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme);
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Enums

    #region Export Window

    /// <summary>
    /// Converts a matching <see cref="ExportationType"/> value and parameter to <see cref="ApplicationColor.Theme"/>, otherwise to <see cref="ApplicationColor.ForegroundDark"/>.
    /// </summary>
    public class MatchingExportationTypeToSolidColorBrushConverter : BaseValueConverter<MatchingExportationTypeToSolidColorBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;
            return (parameter.ToString() == ((ExportationType)value).ToString().ToLower())
                ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme)
                : ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a matching <see cref="ExportationType"/> value and parameter to <see cref="ApplicationColor.Theme"/>, otherwise to <see cref="ApplicationColor.BackgroundIntermediate"/>.
    /// </summary>
    public class MatchingExportationTypeToBorderColorConverter : BaseValueConverter<MatchingExportationTypeToBorderColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return null;
            return (parameter.ToString() == ((ExportationType)value).ToString().ToLower())
                ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme)
                : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    #endregion

    #region Preferences Window

    /// <summary>
    /// Converts a matching <see cref="PreferenceMenuItem"/> value and parameter to <see cref="ApplicationColor.Theme"/>, otherwise to <see cref="ApplicationColor.ForegroundDark"/>.
    /// </summary>
    public class MatchingPreferenceMenuToSolidColorBrushConverter : BaseValueConverter<MatchingPreferenceMenuToSolidColorBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate);
            switch ((PreferenceMenuItem)value)
            {
                case PreferenceMenuItem.General:
                    return parameter.ToString() == "general" ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
                case PreferenceMenuItem.Project:
                    return parameter.ToString() == "project" ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
                default:
                    throw new Exception("Invalid PreferenceMenu.");
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a matching <see cref="PreferenceMenuItem"/> value and parameter to <see cref="ApplicationColor.Theme"/>, otherwise to <see cref="ApplicationColor.BackgroundIntermediate"/>.
    /// </summary>
    public class MatchingPreferenceMenutoBorderColorConverter : BaseValueConverter<MatchingPreferenceMenutoBorderColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
                return ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate);
            switch ((PreferenceMenuItem)value)
            {
                case PreferenceMenuItem.General:
                    return parameter.ToString() == "general" ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate);
                case PreferenceMenuItem.Project:
                    return parameter.ToString() == "project" ? ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme) : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate);
                default:
                    throw new Exception("Invalid PreferenceMenu.");
            }
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }


    #endregion

    #region AudioBufferState

    /// <summary>
    /// Converts an <see cref="AudioBufferState"/> to <see cref="System.Windows.Media.SolidColorBrush"/>.
    /// </summary>
    public class AudioBufferStateToSolidColorBrushConverter : BaseValueConverter<AudioBufferStateToSolidColorBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            AudioBufferState state = (AudioBufferState)value;
            return state switch
            {
                AudioBufferState.Empty => ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundDark),
                AudioBufferState.Loaded => ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundLight),
                AudioBufferState.Selected => ApplicationColorConverter.GetColorBrush(ApplicationColor.Theme),
                AudioBufferState.Playing => ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundLight),
                _ => throw new InvalidEnumValueException(state),
            };
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #endregion

    #region Application Color

    /// <summary>
    /// Converts a <see cref="ApplicationColor"/> to <see cref="System.Windows.Media.Color"/>.
    /// </summary>
    public class ApplicationColorToColorConverter : BaseValueConverter<ApplicationColorToColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ApplicationColorConverter.GetColor((ApplicationColor)value);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ApplicationColor"/> to <see cref="System.Windows.Media.SolidColorBrush"/>.
    /// </summary>
    public class ApplicationColorToSolidColorBrushConverter : BaseValueConverter<ApplicationColorToSolidColorBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ApplicationColorConverter.GetColorBrush((ApplicationColor)value);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Return the <see cref="System.Windows.Media.SolidColorBrush"/> value of the <see cref="ApplicationColor.Theme"/>.
    /// </summary>
    public class ApplicationThemeToBackgroundColorConverter : BaseValueConverter<ApplicationThemeToBackgroundColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ApplicationColorConverter.ConvertToSolidColorBrush(IoC.Get<ProjectModel>().GeneralSettings.Theme, false);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Highlight

    /// <summary>
    /// Converts a true value to <see cref="ApplicationColor.ForegroundLight"/>, otherwise to <see cref="ApplicationColor.Theme"/>.
    /// </summary>
    public class HighlightToBackgroundColorConverter : BaseValueConverter<HighlightToBackgroundColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ApplicationColorConverter.ConvertToSolidColorBrush(IoC.Get<ProjectModel>().GeneralSettings.Theme, (bool)value);
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a boolean to <see cref="ApplicationColor"/> for the <see cref="MidiView"/>'s highlightes notes.
    /// </summary>
    public class HighlightedNoteColorConverter : BaseValueConverter<HighlightedNoteColorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            // foreground
            if (parameter == null)
                return (bool)value ? ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundLight) : ApplicationColorConverter.GetColorBrush(ApplicationColor.ForegroundDark);
            // background
            else
                return (bool)value ? ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundIntermediate) : ApplicationColorConverter.GetColorBrush(ApplicationColor.BackgroundDark);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
