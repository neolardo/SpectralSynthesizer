using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Custom startup to initiliaze the application.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            IoC.Setup();
            Computer.LoadCaches();
            Current.MainWindow = new MainWindow();
        }
    }
}
