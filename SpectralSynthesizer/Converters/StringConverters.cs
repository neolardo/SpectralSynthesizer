using SpectralSynthesizer.Models.Audio.Data;
using System;
using System.Globalization;

namespace SpectralSynthesizer
{
    #region Null or Empty

    /// <summary>
    /// Converts an empty string to "temp".
    /// </summary>
    public class EmptyStringToTempConverter : BaseValueConverter<EmptyStringToTempConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            return String.IsNullOrEmpty(value.ToString()) ? "temp" : value.ToString();
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts an empty string to "untitled".
    /// </summary>
    public class EmptyStringToUntitledConverter : BaseValueConverter<EmptyStringToUntitledConverter>
    {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            return String.IsNullOrEmpty(value.ToString()) ? "untitled" : value.ToString();
        }


        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Loading Window

    /// <summary>
    /// Converts the loading percent to text.
    /// </summary>
    public class LoadingPercentToTextConverter : BaseValueConverter<LoadingPercentToTextConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value * 100.0).ToString("0") + "%";
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Instrument

    /// <summary>
    /// Converts the selected <see cref="Instrument"/> to it's name.
    /// </summary>
    public class SelectedInstrumentToStringConverter : BaseValueConverter<SelectedInstrumentToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return String.Empty;
            else
            {
                Instrument instrument = (Instrument)value;
                return instrument.Name;
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
    /// Converts a <see cref="LoadingStatus"/> to string.
    /// </summary>
    public class WaveToMidiConversionStatusToStringConverter : BaseValueConverter<WaveToMidiConversionStatusToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (LoadingStatus)value switch
            {
                LoadingStatus.Empty => "No midi to show",
                LoadingStatus.Loading => "Converting from wave to midi...",
                LoadingStatus.Loaded => "",
                _ => throw new InvalidEnumValueException((LoadingStatus)value)
            };
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="LoadingStatus"/> to string.
    /// </summary>
    public class NoteGenerationStatusToStringConverter : BaseValueConverter<NoteGenerationStatusToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (LoadingStatus)value switch
            {
                LoadingStatus.Empty => "Load a note of the instrument",
                LoadingStatus.Loading => "Loading note...",
                LoadingStatus.Loaded => "",
                _ => throw new InvalidEnumValueException((LoadingStatus)value)
            };
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts the given <see cref="ApplicationPage"/> value to a string.
    /// </summary>
    public class ApplicationPageToStringConverter : BaseValueConverter<ApplicationPageToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                switch ((ApplicationPage)value)
                {
                    case ApplicationPage.InstrumentBuilder:
                        return "sono";
                    case ApplicationPage.AudioRenderer:
                        return "rend";
                    default:
                        throw new Exception("ApplicationPage unindentified");
                }
            }
            else if (parameter.ToString() == "second")
            {
                switch ((ApplicationPage)value)
                {
                    case ApplicationPage.InstrumentBuilder:
                        return "rend";
                    case ApplicationPage.AudioRenderer:
                        return "sono";
                    default:
                        throw new Exception("ApplicationPage unindentified");
                }
            }
            else
            {
                switch ((ApplicationPage)value)
                {
                    case ApplicationPage.InstrumentBuilder:
                        return "rend";
                    case ApplicationPage.AudioRenderer:
                        return "sono";
                    default:
                        throw new Exception("ApplicationPage unindentified");
                }
            }

        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a <see cref="ExportationState"/> to string.
    /// </summary>
    public class ExportationStateToStringConverter : BaseValueConverter<ExportationStateToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (ExportationState)value switch
            {
                ExportationState.Init => "",
                ExportationState.InProgress => "Exporting...",
                ExportationState.Fail => "Exportation failed.",
                ExportationState.Success => "Exportation successful.",
                _ => throw new InvalidEnumValueException((ExportationState)value)
            };
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region Slider

    /// <summary>
    /// Converts the slider's value to string. The format of the string is based on the converter parameter.
    /// </summary>
    public class SliderValueToStringConverter : BaseValueConverter<SliderValueToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return "n/a";

            string strParameter = parameter as string;
            if (strParameter == "millisecond")
            {
                return $"{(int)((double)value)} ms";
            }
            else if (strParameter == "decibel")
            {
                return string.Format("{0:N1}", (double)value) + " db";
            }
            else if (strParameter == "frequency")
            {
                return string.Format("{0:N1}", (double)value) + " hz";
            }
            else if (strParameter.Contains('.'))
            {
                char decimals = strParameter[strParameter.IndexOf('.') + 1];
                string format = "{0:N" + decimals + "}";
                return string.Format(format, (double)value);
            }
            return "n/a";
        }
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
