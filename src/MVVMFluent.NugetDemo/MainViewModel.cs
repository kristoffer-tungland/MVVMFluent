using MVVMFluent.WPF;
using System.Windows;

namespace MVVMFluent.NugetDemo;

internal class MainViewModel : ValidationViewModelBase
{
    public MainViewModel()
    {
        Input = "Hello World";
    }

    public bool Enable { get => Get(true); set => Set(value); }

    public string? Input
    {
        get => Get<string?>();
        set => When(value).Required().Notify(AsyncCommand, OkCommand).Set();
    }

    public FluentCommand OkCommand => Do(() => ShowDialog(Input)).IfValid(nameof(Input));

    private bool CanExecute()
    {
        return !string.IsNullOrWhiteSpace(Input);
    }

    public bool ThrowException { get => Get(false); set => Set(value); }

    public AsyncFluentCommand AsyncCommand => Do(ShowDialogAsync).If(CanExecute).Handle(HandleException).ConfigureAwait(false);

    private void HandleException(Exception exception)
    {
        if (exception is TaskCanceledException)
            MessageBox.Show(exception.Message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async Task ShowDialogAsync(CancellationToken cancellationToken)
    {
        for (var i = 1; i <= 100; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            await Task.Delay(50, cancellationToken);
            AsyncCommand.ReportProgress(i + 1, 100);

            if (AsyncCommand.Progress == 50 && ThrowException)
                throw new Exception("Something went wrong");

        }
        ShowDialog(Input);
    }

    public FluentCommand<string> HelpCommand => Do<string>(ShowDialog);

    private void ShowDialog(string? input)
    {
        MessageBox.Show(input);
    }
}