using System.Windows;

namespace MVVMFluent.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        Closed += OnClosed;
        DataContext = new MainViewModel();
        InitializeComponent();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        Closed -= OnClosed;
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}