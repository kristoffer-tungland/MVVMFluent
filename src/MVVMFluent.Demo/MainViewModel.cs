using System.Windows;

namespace MVVMFluent.Demo;

internal class MainViewModel : ViewModelBase
{
    public bool Enable { get => Get(true); set => Set(value); }

    public string? Input
    {
        get => Get<string?>();
        set => When(value).Notify(OkCommand, HelpCommand).Set();
    }

    public Command OkCommand => Do(() => ShowDialog(Input)).If(() => !string.IsNullOrWhiteSpace(Input));

    public Command<string> HelpCommand => Do<string>(ShowDialog);

    private void ShowDialog(string? input)
    {
        MessageBox.Show(input);
    }
}