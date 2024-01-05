using System.Windows;

namespace SnakL
{
    public partial class App : Application
    {
        public App() => this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = false;
            MessageBox.Show(string.Format("An unhandled exception occurred: {0}", e.Exception.ToString()), "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
