using System.Windows;

namespace MVVMFluent.NugetDemo;

internal class MainViewModel : ViewModelBase
{
    public bool Enable { get => Get(true); set => Set(value); }

    public string? Input
    {
        get => Get<string?>();
        set => When(value).Notify(OkCommand, AsyncCommand).Set();
    }

    public Command OkCommand => Do(() => ShowDialog(Input)).If(CanExecute);

    private bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(Input);
    }

    public bool ThrowException { get => Get(false); set => Set(value); }

    public AsyncCommand AsyncCommand => Do(ShowDialogAsync).If(CanExecute).OnException(HandleException).ConfigureAwait(false);

    private void HandleException(Exception exception)
    {
        MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task ShowDialogAsync()
    {
        for (var i = 1; i <= 100; i++)
        {
            if (AsyncCommand.IsCancellationRequested)
                return;

            await Task.Delay(50);
            AsyncCommand.ReportProgress(i + 1, 100);

            if (AsyncCommand.Progress == 50 && ThrowException)
                throw new Exception("Something went wrong");

        }
        ShowDialog(Input);
    }

    public Command<string> HelpCommand => Do<string>(ShowDialog);

    private void ShowDialog(string? input)
    {
        MessageBox.Show(input);
    }
}