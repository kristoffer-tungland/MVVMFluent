using MVVMFluent.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Commands;

/// <summary>
/// Represents an asynchronous command that supports cancellation and tracks execution state.
/// </summary>
public class AsyncFluentCommand : IAsyncFluentCommand, INotifyPropertyChanged, IDisposable
{
    private Func<object?, CancellationToken, Task>? _execute;
    private Func<object?, bool>? _canExecute;
    private CancellationTokenSource? _cts;
    private Action<Exception>? _onException;
    private bool _continueOnCapturedContext = true;
    private FluentCommand? _cancelCommand;
    private bool _disposed;
    private bool _isRunning;
    private int _progress;

    public AsyncFluentCommand()
    {
        PropertyChanged += OnSelfPropertyChanged;
    }

    public event EventHandler? CanExecuteChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
            RaiseCanExecuteChanged();
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            if (_progress == value)
            {
                return;
            }

            _progress = value;
            OnPropertyChanged();
        }
    }

    public IFluentSetterViewModel? Owner { get; private set; }

    public bool IsBuilt { get; private set; }

    public IFluentCommand CancelCommand
    {
        get
        {
            if (_cancelCommand != null)
            {
                return _cancelCommand;
            }

            _cancelCommand = FluentCommand.Do(Cancel, Owner)
                                         .If(() => IsRunning && !IsCancellationRequested);
            return _cancelCommand;
        }
    }

    public CancellationTokenSource? CancellationTokenSource => _cts;

    public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

    public void MarkAsBuilt() => IsBuilt = true;

    protected void SetExecute(Func<object?, CancellationToken, Task> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    public static AsyncFluentCommand Do(Func<Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand { Owner = owner };
        command.SetExecute((_, _) => execute());
        return command;
    }

    public static AsyncFluentCommand Do(Func<object?, Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand { Owner = owner };
        command.SetExecute((o, _) => execute(o));
        return command;
    }

    public static AsyncFluentCommand Do(Func<object?, CancellationToken, Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand { Owner = owner };
        command.SetExecute(execute);
        return command;
    }

    public AsyncFluentCommand If(Func<bool> canExecute) => If(_ => canExecute());

    public AsyncFluentCommand If(Func<object?, bool> canExecute)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (canExecute == null)
        {
            throw new ArgumentNullException(nameof(canExecute));
        }
        _canExecute = canExecute;
        return this;
    }

    public AsyncFluentCommand IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    public AsyncFluentCommand Handle(Action<Exception> handle)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (handle == null)
        {
            throw new ArgumentNullException(nameof(handle));
        }
        _onException = handle;
        return this;
    }

    public AsyncFluentCommand ConfigureAwait(bool continueOnCapturedContext)
    {
        if (IsBuilt)
        {
            return this;
        }

        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    public bool CanExecute(object? parameter)
        => !IsRunning && (_canExecute?.Invoke(parameter) ?? true);

    public async Task ExecuteAsync(object? parameter)
    {
        if (_execute == null)
        {
            throw new InvalidOperationException("No action has been specified for the command.");
        }

        if (!CanExecute(parameter))
        {
            return;
        }

        var linkedCts = new CancellationTokenSource();
        _cts = linkedCts;
        IsRunning = true;
        Progress = 0;

        try
        {
            await _execute(parameter, linkedCts.Token).ConfigureAwait(_continueOnCapturedContext);
        }
        finally
        {
            Progress = 0;
            IsRunning = false;
            _cts = null;
            linkedCts.Dispose();
        }
    }

    public void Execute(object? parameter)
    {
        ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
    }

    public void Cancel()
    {
        if (IsRunning && _cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
        }
    }

    public void ReportProgress(int progress)
    {
        Progress = progress;
    }

    public void ReportProgress(int current, int total)
    {
        if (total <= 0)
        {
            ReportProgress(0);
            return;
        }

        var ratio = Math.Min(1d, Math.Max(0d, (double)current / total));
        ReportProgress((int)(ratio * 100));
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _cancelCommand?.RaiseCanExecuteChanged();
    }

    private bool HasNoErrors(string[] propertyNames)
    {
        if (Owner is not IValidationFluentSetterViewModel viewModel)
        {
            throw new InvalidOperationException(
                "Validation commands require a view model derived from ValidationViewModelBase.");
        }

        foreach (var propertyName in propertyNames)
        {
            var builder = viewModel.GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;
            if (builder?.HasErrors == true)
            {
                return false;
            }
        }

        return true;
    }

    private static void EnsurePropertyNames(string[] propertyNames)
    {
        if (propertyNames == null)
        {
            throw new ArgumentNullException(nameof(propertyNames));
        }

        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property name must be provided.", nameof(propertyNames));
        }

        foreach (var propertyName in propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property names cannot be null or whitespace.", nameof(propertyNames));
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            PropertyChanged -= OnSelfPropertyChanged;
            _cancelCommand?.Dispose();
            _cancelCommand = null;
            _cts?.Dispose();
            _cts = null;
            _execute = null;
            _canExecute = null;
            _onException = null;
            CanExecuteChanged = null;
        }

        _disposed = true;
    }
}

/// <summary>
/// Represents an asynchronous command that supports cancellation and tracks execution state, with a generic parameter.
/// </summary>
public class AsyncFluentCommand<T> : IAsyncFluentCommand<T>, INotifyPropertyChanged, IDisposable
{
    private Func<T?, CancellationToken, Task>? _execute;
    private Func<T?, bool>? _canExecute;
    private CancellationTokenSource? _cts;
    private Action<Exception>? _onException;
    private bool _continueOnCapturedContext = true;
    private FluentCommand? _cancelCommand;
    private bool _disposed;
    private bool _isRunning;
    private int _progress;

    public AsyncFluentCommand()
    {
        PropertyChanged += OnSelfPropertyChanged;
    }

    public event EventHandler? CanExecuteChanged;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
            RaiseCanExecuteChanged();
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            if (_progress == value)
            {
                return;
            }

            _progress = value;
            OnPropertyChanged();
        }
    }

    public IFluentSetterViewModel? Owner { get; private set; }

    public bool IsBuilt { get; private set; }

    public IFluentCommand CancelCommand
    {
        get
        {
            if (_cancelCommand != null)
            {
                return _cancelCommand;
            }

            _cancelCommand = FluentCommand.Do(Cancel, Owner)
                                         .If(() => IsRunning && !IsCancellationRequested);
            return _cancelCommand;
        }
    }

    public CancellationTokenSource? CancellationTokenSource => _cts;

    public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

    public void MarkAsBuilt() => IsBuilt = true;

    protected void SetExecute(Func<T?, CancellationToken, Task> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    public static AsyncFluentCommand<T> Do(Func<T?, Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand<T> { Owner = owner };
        command.SetExecute((value, _) => execute(value));
        return command;
    }

    public static AsyncFluentCommand<T> Do(Func<T?, CancellationToken, Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand<T> { Owner = owner };
        command.SetExecute(execute);
        return command;
    }

    public AsyncFluentCommand<T> If(Func<bool> canExecute) => If(_ => canExecute());

    public AsyncFluentCommand<T> If(Func<T?, bool> canExecute)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (canExecute == null)
        {
            throw new ArgumentNullException(nameof(canExecute));
        }
        _canExecute = canExecute;
        return this;
    }

    public AsyncFluentCommand<T> IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    public AsyncFluentCommand<T> Handle(Action<Exception> handle)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (handle == null)
        {
            throw new ArgumentNullException(nameof(handle));
        }
        _onException = handle;
        return this;
    }

    public AsyncFluentCommand<T> ConfigureAwait(bool continueOnCapturedContext)
    {
        if (IsBuilt)
        {
            return this;
        }

        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    public bool CanExecute(object? parameter)
    {
        var typedParameter = parameter is T value ? value : default;
        return !IsRunning && (_canExecute?.Invoke(typedParameter) ?? true);
    }

    public Task ExecuteAsync(object? parameter)
    {
        return ExecuteAsync(parameter is T value ? value : default);
    }

    public async Task ExecuteAsync(T? parameter)
    {
        if (_execute == null)
        {
            throw new InvalidOperationException("No action has been specified for the command.");
        }

        if (!CanExecute(parameter))
        {
            return;
        }

        var linkedCts = new CancellationTokenSource();
        _cts = linkedCts;
        IsRunning = true;
        Progress = 0;

        try
        {
            await _execute(parameter, linkedCts.Token).ConfigureAwait(_continueOnCapturedContext);
        }
        finally
        {
            Progress = 0;
            IsRunning = false;
            _cts = null;
            linkedCts.Dispose();
        }
    }

    public void Execute(object? parameter)
    {
        ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
    }

    public void Cancel()
    {
        if (IsRunning && _cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
        }
    }

    public void ReportProgress(int progress)
    {
        Progress = progress;
    }

    public void ReportProgress(int current, int total)
    {
        if (total <= 0)
        {
            ReportProgress(0);
            return;
        }

        var ratio = Math.Min(1d, Math.Max(0d, (double)current / total));
        ReportProgress((int)(ratio * 100));
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnSelfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        _cancelCommand?.RaiseCanExecuteChanged();
    }

    private bool HasNoErrors(string[] propertyNames)
    {
        if (Owner is not IValidationFluentSetterViewModel viewModel)
        {
            throw new InvalidOperationException(
                "Validation commands require a view model derived from ValidationViewModelBase.");
        }

        foreach (var propertyName in propertyNames)
        {
            var builder = viewModel.GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;
            if (builder?.HasErrors == true)
            {
                return false;
            }
        }

        return true;
    }

    private static void EnsurePropertyNames(string[] propertyNames)
    {
        if (propertyNames == null)
        {
            throw new ArgumentNullException(nameof(propertyNames));
        }

        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property name must be provided.", nameof(propertyNames));
        }

        foreach (var propertyName in propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property names cannot be null or whitespace.", nameof(propertyNames));
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            PropertyChanged -= OnSelfPropertyChanged;
            _cancelCommand?.Dispose();
            _cancelCommand = null;
            _cts?.Dispose();
            _cts = null;
            _execute = null;
            _canExecute = null;
            _onException = null;
            CanExecuteChanged = null;
        }

        _disposed = true;
    }
}
