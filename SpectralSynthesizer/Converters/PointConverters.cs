using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Converts a <see cref="ObservableCollection{T}"/> of <see cref="Point"/>s to a <see cref="PointCollection"/>.
    /// </summary>
    public class ObservableCollectionToPointCollectionConverter : BaseValueConverter<ObservableCollectionToPointCollectionConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            var points = value as ObservableCollection<Point>;
            var pc = new PointCollection(points);
            return pc;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}