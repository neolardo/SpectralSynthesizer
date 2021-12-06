using SpectralSynthesizer.Models;
using System;
using System.Windows.Media;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A static converter for <see cref="ApplicationColor"/>.
    /// </summary>
    public static class ApplicationColorConverter
    {
        /// <summary>
        /// Gets the correspoding <see cref="SolidColorBrush"/> from the <see cref="ApplicationColor"/>.
        /// </summary>
        /// <param name="color">The <see cref="ApplicationColor"/>.</param>
        /// <returns>The <see cref="SolidColorBrush"/>.</returns>
        public static SolidColorBrush GetColorBrush(ApplicationColor color) => new SolidColorBrush(GetColor(color));

        /// <summary>
        /// Gets the correspoding <see cref="Color"/> from the <see cref="ApplicationColor"/>.
        /// </summary>
        /// <param name="color">The <see cref="ApplicationColor"/>.</param>
        /// <returns>The <see cref="Color"/>.</returns>
        public static Color GetColor(ApplicationColor color)
        {
            return color switch
            {
                ApplicationColor.BackgroundDark => (Color)ColorConverter.ConvertFromString("#1D1D1D"),
                ApplicationColor.BackgroundIntermediate => (Color)ColorConverter.ConvertFromString("#2A2A2A"),
                ApplicationColor.BackgroundLight => (Color)ColorConverter.ConvertFromString("#3B3B3B"),
                ApplicationColor.ForegroundDark => (Color)ColorConverter.ConvertFromString("#6B6B6B"),
                ApplicationColor.ForegroundIntermediate => (Color)ColorConverter.ConvertFromString("#BBBBBB"),
                ApplicationColor.ForegroundLight => (Color)ColorConverter.ConvertFromString("#DDDDDD"),
                ApplicationColor.Theme => ConvertToColor(IoC.Get<ProjectModel>().GeneralSettings.Theme),
                _ => throw new InvalidOperationException("ApplicationColor is not valid."),
            };
        }

        /// <summary>
        /// Converts a an <see cref="ApplicationTheme"/> to <see cref="SolidColorBrush"/>.
        /// </summary>
        /// <param name="theme">The theme of the application</param>
        /// <param name="highlight">True if the highlighted version is needed.</param>
        public static SolidColorBrush ConvertToSolidColorBrush(ApplicationTheme theme, bool highlight = false) => new SolidColorBrush(ConvertToColor(theme, highlight));

        /// <summary>
        /// Converts a an <see cref="ApplicationTheme"/> to <see cref="Color"/>.
        /// </summary>
        /// <param name="theme">The theme of the application.</param>
        /// <param name="highlight">True if the highlighted version is needed.</param
        public static Color ConvertToColor(ApplicationTheme theme, bool highlight = false)
        {
            if (highlight)
            {
                return (Color)ColorConverter.ConvertFromString("#DDDDDD");
            }
            else
            {
                return theme switch
                {
                    ApplicationTheme.Misty => (Color)ColorConverter.ConvertFromString("#3D98C7"),
                    ApplicationTheme.Neon => (Color)ColorConverter.ConvertFromString("#45CA3C"),
                    ApplicationTheme.Exotic => (Color)ColorConverter.ConvertFromString("#CD9245"),
                    ApplicationTheme.Radiant => (Color)ColorConverter.ConvertFromString("#CDBa33"),
                    ApplicationTheme.Pretty => (Color)ColorConverter.ConvertFromString("#CA72A0"),
                    _ => throw new InvalidOperationException("ApplicationTheme is not valid.")
                };
            }
        }
    }
}
