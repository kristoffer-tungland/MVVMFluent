using System.Configuration;
using System.Data;
using System.Windows;

namespace MVVMFluent.Demo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Catch all unhandled exceptions
    public App()
    {
        DispatcherUnhandledException += (sender, e) =>
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };
    }
}