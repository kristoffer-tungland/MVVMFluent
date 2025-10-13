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

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFluentCommand"/> class.
    /// </summary>
    public AsyncFluentCommand()
    {
        PropertyChanged += OnSelfPropertyChanged;
    }

    /// <summary>
    /// Occurs when changes affecting the ability of the command to execute should be re-evaluated.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Occurs when a property value on the command changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets a value indicating whether the command is currently running.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the progress of the command execution as a percentage between 0 and 100.
    /// </summary>
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

    /// <summary>
    /// Gets the view model that owns this command, if any.
    /// </summary>
    public IFluentSetterViewModel? Owner { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the command has been fully configured and built.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Gets a command that can be used to cancel the currently running operation.
    /// </summary>
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

    /// <summary>
    /// Gets the <see cref="CancellationTokenSource"/> that controls cancellation for the current execution, or <see langword="null"/> if the command is idle.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource => _cts;

    /// <summary>
    /// Gets a value indicating whether cancellation has been requested for the current execution.
    /// </summary>
    public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

    /// <summary>
    /// Marks the command as built, preventing further configuration changes.
    /// </summary>
    public void MarkAsBuilt() => IsBuilt = true;

    /// <summary>
    /// Sets the delegate that will be executed when the command runs.
    /// </summary>
    /// <param name="execute">The delegate to execute. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    protected void SetExecute(Func<object?, CancellationToken, Task> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    /// <summary>
    /// Creates a new asynchronous command that executes the specified delegate without a parameter or cancellation token.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="AsyncFluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Creates a new asynchronous command that executes the specified delegate with a parameter but without a cancellation token.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="AsyncFluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Creates a new asynchronous command that executes the specified delegate with a parameter and cancellation token.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="AsyncFluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand"/> instance.</returns>
    public AsyncFluentCommand If(Func<bool> canExecute) => If(_ => canExecute());

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/> for the given parameter.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canExecute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures the command to only execute when the specified properties are free of validation errors.
    /// </summary>
    /// <param name="propertyNames">The property names that must be valid before the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand"/> instance.</returns>
    public AsyncFluentCommand IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    /// <summary>
    /// Registers an exception handler that is invoked when the asynchronous execution fails.
    /// </summary>
    /// <param name="handle">The exception handler to invoke.</param>
    /// <returns>The current <see cref="AsyncFluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures whether continuations should capture the current synchronization context.
    /// </summary>
    /// <param name="continueOnCapturedContext">A value indicating whether to resume on the captured context.</param>
    /// <returns>The current <see cref="AsyncFluentCommand"/> instance.</returns>
    public AsyncFluentCommand ConfigureAwait(bool continueOnCapturedContext)
    {
        if (IsBuilt)
        {
            return this;
        }

        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    /// <summary>
    /// Determines whether the command can execute using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to evaluate.</param>
    /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter)
        => !IsRunning && (_canExecute?.Invoke(parameter) ?? true);

    /// <summary>
    /// Executes the command asynchronously using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no execution delegate has been configured.</exception>
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

    /// <summary>
    /// Executes the command asynchronously and observes exceptions using the configured handler.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    public void Execute(object? parameter)
    {
        ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
    }

    /// <summary>
    /// Requests cancellation of the current operation, if one is running.
    /// </summary>
    public void Cancel()
    {
        if (IsRunning && _cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
        }
    }

    /// <summary>
    /// Reports progress by setting the <see cref="Progress"/> property directly.
    /// </summary>
    /// <param name="progress">The progress percentage between 0 and 100.</param>
    public void ReportProgress(int progress)
    {
        Progress = progress;
    }

    /// <summary>
    /// Reports progress by calculating the completion percentage from the supplied values.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="total">The total progress value.</param>
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

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the command's execution state may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Releases the resources used by the command.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the command and optionally disposes of managed resources.
    /// </summary>
    /// <param name="disposing">A value indicating whether to dispose managed resources.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncFluentCommand{T}"/> class.
    /// </summary>
    public AsyncFluentCommand()
    {
        PropertyChanged += OnSelfPropertyChanged;
    }

    /// <summary>
    /// Occurs when changes affecting the ability of the command to execute should be re-evaluated.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Occurs when a property value on the command changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets a value indicating whether the command is currently running.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the progress of the command execution as a percentage between 0 and 100.
    /// </summary>
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

    /// <summary>
    /// Gets the view model that owns this command, if any.
    /// </summary>
    public IFluentSetterViewModel? Owner { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the command has been fully configured and built.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Gets a command that can be used to cancel the currently running operation.
    /// </summary>
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

    /// <summary>
    /// Gets the <see cref="CancellationTokenSource"/> that controls cancellation for the current execution, or <see langword="null"/> if the command is idle.
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource => _cts;

    /// <summary>
    /// Gets a value indicating whether cancellation has been requested for the current execution.
    /// </summary>
    public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

    /// <summary>
    /// Marks the command as built, preventing further configuration changes.
    /// </summary>
    public void MarkAsBuilt() => IsBuilt = true;

    /// <summary>
    /// Sets the delegate that will be executed when the command runs.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    protected void SetExecute(Func<T?, CancellationToken, Task> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    /// <summary>
    /// Creates a new asynchronous command that executes the specified delegate.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    public static AsyncFluentCommand<T> Do(Func<T?, Task> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new AsyncFluentCommand<T> { Owner = owner };
        command.SetExecute((o, _) => execute(o));
        return command;
    }

    /// <summary>
    /// Creates a new asynchronous command that executes the specified delegate with cancellation support.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    public AsyncFluentCommand<T> If(Func<bool> canExecute) => If(_ => canExecute());

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/> for the given parameter.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canExecute"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures the command to only execute when the specified properties are free of validation errors.
    /// </summary>
    /// <param name="propertyNames">The property names that must be valid before the command can execute.</param>
    /// <returns>The current <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    public AsyncFluentCommand<T> IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    /// <summary>
    /// Registers an exception handler that is invoked when the asynchronous execution fails.
    /// </summary>
    /// <param name="handle">The exception handler to invoke.</param>
    /// <returns>The current <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handle"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Configures whether continuations should capture the current synchronization context.
    /// </summary>
    /// <param name="continueOnCapturedContext">A value indicating whether to resume on the captured context.</param>
    /// <returns>The current <see cref="AsyncFluentCommand{T}"/> instance.</returns>
    public AsyncFluentCommand<T> ConfigureAwait(bool continueOnCapturedContext)
    {
        if (IsBuilt)
        {
            return this;
        }

        _continueOnCapturedContext = continueOnCapturedContext;
        return this;
    }

    /// <summary>
    /// Determines whether the command can execute using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to evaluate.</param>
    /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter)
    {
        var typedParameter = parameter is T value ? value : default;
        return !IsRunning && (_canExecute?.Invoke(typedParameter) ?? true);
    }

    /// <summary>
    /// Executes the command asynchronously using the supplied parameter, converting it to the expected type when possible.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    public Task ExecuteAsync(object? parameter)
    {
        return ExecuteAsync(parameter is T value ? value : default);
    }

    /// <summary>
    /// Executes the command asynchronously using the supplied strongly-typed parameter.
    /// </summary>
    /// <param name="parameter">The strongly-typed parameter to pass to the execution delegate.</param>
    /// <returns>A task that represents the asynchronous execution.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no execution delegate has been configured.</exception>
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

    /// <summary>
    /// Executes the command asynchronously and observes exceptions using the configured handler.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    public void Execute(object? parameter)
    {
        ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
    }

    /// <summary>
    /// Requests cancellation of the current operation, if one is running.
    /// </summary>
    public void Cancel()
    {
        if (IsRunning && _cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
        }
    }

    /// <summary>
    /// Reports progress by setting the <see cref="Progress"/> property directly.
    /// </summary>
    /// <param name="progress">The progress percentage between 0 and 100.</param>
    public void ReportProgress(int progress)
    {
        Progress = progress;
    }

    /// <summary>
    /// Reports progress by calculating the completion percentage from the supplied values.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="total">The total progress value.</param>
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

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the command's execution state may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyName"/> is <see langword="null"/>.</exception>
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

    /// <summary>
    /// Releases the resources used by the command.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the command and optionally disposes of managed resources.
    /// </summary>
    /// <param name="disposing">A value indicating whether to dispose managed resources.</param>
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
