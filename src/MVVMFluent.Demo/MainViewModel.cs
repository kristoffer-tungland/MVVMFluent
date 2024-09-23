using System.Windows;

namespace MVVMFluent.Demo;

internal class MainViewModel : ViewModelBase
{
    public bool Enable { get => Get(true); set => Set(value); }

    public string? Input
    {
        get => Get<string?>();
        set => When(value).Notify(OkCommand, HelpCommand, AsyncCommand).Set();
    }

    public Command OkCommand => Do(() => ShowDialog(Input)).If(() => !string.IsNullOrWhiteSpace(Input));


    public bool ThrowException { get => Get(false); set => Set(value); }
    public int Progress { get => Get(0); set => Set(value); }
    public Command CancelAsyncCommand => Cancel(AsyncCommand);

    public AsyncCommand AsyncCommand => Do(ShowDialogAsync).If(() => !string.IsNullOrWhiteSpace(Input)).OnException(ex => MessageBox.Show(ex.Message));

    private async Task ShowDialogAsync()
    {
        try
        {
            for (var i = 1; i < 100; i++)
            {
                if (AsyncCommand.IsCancellationRequested)
                    return;

                await Task.Delay(50);
                Progress = i+1;

                if (Progress == 50 && ThrowException)
                    throw new Exception("Something went wrong");
            }
            ShowDialog(Input);
        }
        finally
        {
            Progress = 0;
        }
    }

    public Command<string> HelpCommand => Do<string>(ShowDialog);

    private void ShowDialog(string? input)
    {
        MessageBox.Show(input);
    }
}