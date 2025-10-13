using System.Windows;
using System.Windows.Input;

namespace MVVMFluent.Demo;

internal class MainViewModel : ValidationViewModelBase
{
    public bool Enable { get => Get(true); set => Set(value); }
    public bool ThrowException { get => Get(false); set => Set(value); }

    public string? Input
    {
        get => Get(defaultValue: "Hello World");
        set => When(value)
            .HasValue()
            .HasMinLength(5, "Input must be at least 5 characters long")
            .Notify(AsyncFluentCommand, OkCommand)
            .Set();
    }

    public ICommand OkCommand => Do(() => ShowDialog(Input)).IfValid(nameof(Input));
    
    public IFluentCommand<string> HelpCommand => Do<string>(ShowDialog);

    public IAsyncFluentCommand AsyncFluentCommand => Do(ShowDialogAsync)
        .If(() => HasErrors == false)
        .Handle(HandleException);

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
            AsyncFluentCommand.ReportProgress(i+1, 100);

            if (AsyncFluentCommand.Progress == 50 && ThrowException)
                throw new Exception("Something went wrong");

        }
        ShowDialog(Input);
    }


    private void ShowDialog(string? input)
    {
        MessageBox.Show(input);
    }
}