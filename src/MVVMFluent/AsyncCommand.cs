namespace MVVMFluent
{
    /// <summary>
    /// Defines an asynchronous fluent command that supports cancellation and tracking of execution state.
    /// </summary>
    public interface IAsyncFluentCommand : IFluentCommand, global::System.IDisposable
    {
        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets or sets the current progress of the command.
        /// </summary>
        int Progress { get; set; }

        /// <summary>
        /// Sets the current progress of the command.
        /// </summary>
        void ReportProgress(int progress);

        /// <summary>
        /// Reports the progress of the command.
        /// </summary>
        void ReportProgress(int current, int total);

        /// <summary>
        /// Gets the command to cancel the current operation.
        /// </summary>
        Command CancelCommand { get; }

        /// <summary>
        /// Indicates whether the command is requested to be cancelled.
        /// </summary>
        bool IsCancellationRequested { get; }

        /// <summary>
        /// Provides the cancellation token source used to cancel the current operation.
        /// </summary>
        global::System.Threading.CancellationTokenSource? CancellationTokenSource { get; }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">Optional parameter for the command execution.</param>
        global::System.Threading.Tasks.Task ExecuteAsync(object? parameter);

        /// <summary>
        /// Cancels the current operation if it is running.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Represents an asynchronous command that supports cancellation and tracks execution state.
    /// <example>
    /// <code lang="csharp">
    /// public AsyncCommand OkCommand => Do(async () => await Task.Delay(1000)).If(() => CanOk).OnException(ex => MessageBox.Show(ex.Message)).ConfigureAwait(false);
    /// 
    /// public bool CanOk { get => Get(true); set => Set(value); }
    /// </code>
    /// <code lang="xaml">
    /// &lt;Button Content=&quot;Run&quot; Command=&quot;{Binding AsyncCommand}&quot; /&gt;
    /// &lt;ProgressBar Visibility = &quot;{Binding AsyncCommand.IsRunning, Converter={StaticResource BoolToVisibilityConverter}}&quot; /&gt;
    /// &lt;Button Content=&quot;Cancel&quot; Command=&quot;{Binding AsyncCommand.CancelCommand}&quot; /&gt;
    /// </code>
    /// </example>
    /// </summary>
    public class AsyncCommand : IAsyncFluentCommand, global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private global::System.Func<object?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task>? _execute;
        private global::System.Func<object?, bool>? _canExecute;
        private bool _isRunning;
        private global::System.Threading.CancellationTokenSource? _cts;
        private global::System.Action<global::System.Exception>? _onException;
        private bool _continueOnCapturedContext = true;
        private bool _disposed;
        private Command? _cancelCommand;
        private int _progress = 0;
        public IFluentSetterViewModel? Owner { get; private set; }

        public bool IsBuilt { get; private set; }

        public void MarkAsBuilt()
        {
            IsBuilt = true;
        }

        protected void SetExecute(global::System.Func<object?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute)
        {
            _execute = execute;
        }

        /// <summary>
        /// Event raised when the ability to execute changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command to cancel the current operation.
        /// </summary>
        public Command CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = Command.Do(() => Cancel(), Owner).If(() => IsRunning && _cts?.IsCancellationRequested == false);
                    PropertyChanged += (s, e) => _cancelCommand.RaiseCanExecuteChanged();
                }
                return _cancelCommand;
            }
        }

        /// <summary>
        /// Gets whether the command is currently executing.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                    RaiseCanExecuteChanged();
                }
            }
        }

        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sets the current progress of the command.
        /// </summary>
        public void ReportProgress(int progress) => Progress = progress;

        /// <summary>
        /// Reports the progress of the command.
        /// </summary>
        /// <param name="current">The current iten processed.</param>
        /// <param name="total">Total number of items to process.</param>
        public void ReportProgress(int current, int total)
        {
            ReportProgress((int)((double)current / total * 100));
        }

        /// <summary>
        /// Gets the cancellation token source used to cancel the command.
        /// </summary>
        public global::System.Threading.CancellationTokenSource? CancellationTokenSource => _cts;

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        /// <returns>True if the command can execute; otherwise false.</returns>
        public bool CanExecute(object? parameter)
        {
            return !IsRunning && (_canExecute == null || _canExecute(parameter));
        }

        /// <summary>
        /// Checks if the command is requested to be cancelled.
        /// </summary>
        public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public async global::System.Threading.Tasks.Task ExecuteAsync(object? parameter)
        {
            if (_execute == null)
                throw new global::System.InvalidOperationException("No action has been specified for the command.");

            if (CanExecute(parameter))
            {
                _cts = new global::System.Threading.CancellationTokenSource();
                IsRunning = true;
                Progress = 0;

                try
                {
                    await _execute(parameter, _cts.Token);
                }
                finally
                {
                    Progress = 0;
                    IsRunning = false;
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="parameter">The command parameter.</param>
        public void Execute(object? parameter)
        {
            ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
        }

        /// <summary>
        /// Cancels the currently running command.
        /// </summary>
        public void Cancel()
        {
            if (IsRunning && _cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Do(global::System.Func<global::System.Threading.Tasks.Task> execute, IFluentSetterViewModel? owner)
        {
            return Do((_, _) => execute(), owner);
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Do(global::System.Func<object?, global::System.Threading.Tasks.Task> execute, IFluentSetterViewModel? owner)
        {
            var command = new AsyncCommand
            {
                Owner = owner
            };
            command.SetExecute((o, _) => execute(o));
            return command;
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand"/>.</returns>
        public static AsyncCommand Do(global::System.Func<object?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute, IFluentSetterViewModel? owner)
        {
            var command = new AsyncCommand
            {
                Owner = owner
            };
            command.SetExecute(execute);
            return command;
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute with a parameter.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand If(global::System.Func<object?, bool> canExecute)
        {
            if (IsBuilt)
                return this;

            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Specifies an action to handle exceptions during execution.
        /// </summary>
        /// <param name="onException">The action to handle exceptions.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand OnException(global::System.Action<global::System.Exception> onException)
        {
            if (IsBuilt)
                return this;

            _onException = onException;
            return this;
        }

        /// <summary>
        /// Sets whether the execution should continue on the captured synchronization context.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to continue on captured context.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand ConfigureAwait(bool continueOnCapturedContext)
        {
            if (IsBuilt)
                return this;

            _continueOnCapturedContext = continueOnCapturedContext;
            return this;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null.");

            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);

        /// <summary>
        /// Releases resources used by the command.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the command and any related resources.
        /// </summary>
        /// <param name="disposing">Whether the method is called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _execute = null;
                    _canExecute = null;
                    _onException = null;
                    _continueOnCapturedContext = false;

                    CanExecuteChanged = null;
                    PropertyChanged = null;
                    _cts?.Dispose();
                    _cts = null;
                    _cancelCommand?.Dispose();
                    _cancelCommand = null;
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for the <see cref="AsyncCommand"/> class.
        /// </summary>
        ~AsyncCommand()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Represents an asynchronous command that supports cancellation and tracks execution state, with a generic parameter.
    /// <example>
    /// <code lang="csharp">
    /// public AsyncCommand OkCommand => Do&lt;string&gt;(Ok).If(() => CanOk).OnException(ex => MessageBox.Show(ex.Message)).ConfigureAwait(true);
    /// 
    /// public async Task Ok(string input)
    /// {
    ///    await Task.Delay(1000);
    ///    MessageBox.Show(input);
    /// }
    /// 
    /// public bool CanOk { get => Get(true); set => Set(value); }
    /// </code>
    /// <code lang="xaml">
    /// &lt;Button Content=&quot;Run&quot; Command=&quot;{Binding AsyncCommand}&quot; /&gt;
    /// &lt;ProgressBar Visibility = &quot;{Binding AsyncCommand.IsRunning, Converter={StaticResource BoolToVisibilityConverter}}&quot; /&gt;
    /// &lt;Button Content=&quot;Cancel&quot; Command=&quot;{Binding AsyncCommand.CancelCommand}&quot; /&gt;
    /// </code>
    /// </example>
    /// </summary>
    public class AsyncCommand<T> : IAsyncFluentCommand, global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private global::System.Func<T?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task>? _execute;
        private global::System.Func<T?, bool>? _canExecute;
        private bool _isRunning;
        private global::System.Threading.CancellationTokenSource? _cts;
        private global::System.Action<global::System.Exception>? _onException;
        private bool _continueOnCapturedContext = true;
        private bool _disposed;
        private Command? _cancelCommand;
        private int _progress = 0;
        public IFluentSetterViewModel? Owner { get; private set; }

        public bool IsBuilt { get; private set; }

        public void MarkAsBuilt()
        {
            IsBuilt = true;
        }

        protected void SetExecute(global::System.Func<T?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute)
        {
            _execute = execute;
        }

        /// <summary>
        /// Event raised when the ability to execute changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the command to cancel the current operation.
        /// </summary>
        public Command CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = Command.Do(() => Cancel(), Owner).If(() => IsRunning && _cts?.IsCancellationRequested == false);
                    PropertyChanged += (s, e) => _cancelCommand.RaiseCanExecuteChanged();
                }
                return _cancelCommand;
            }
        }

        /// <summary>
        /// Gets whether the command is currently executing.
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                    RaiseCanExecuteChanged();
                }
            }
        }

        public int Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sets the current progress of the command.
        /// </summary>
        public void ReportProgress(int progress) => Progress = progress;

        /// <summary>
        /// Reports the progress of the command.
        /// </summary>
        /// <param name="current">The current iten processed.</param>
        /// <param name="total">Total number of items to process.</param>
        public void ReportProgress(int current, int total)
        {
            ReportProgress((int)((double)current / total * 100));
        }

        /// <summary>
        /// Gets the cancellation token source used to cancel the command.
        /// </summary>
        public global::System.Threading.CancellationTokenSource? CancellationTokenSource => _cts;

        public bool CanExecute(object parameter)
        {
            return CanExecute((T?)parameter);
        }

        /// <summary>
        /// Determines whether the command can execute.
        /// </summary>
        /// <param name="parameter">The command parameter of type <typeparamref name="T"/>.</param>
        /// <returns>True if the command can execute; otherwise false.</returns>
        private bool CanExecute(T? parameter)
        {
            return !IsRunning && (_canExecute == null || _canExecute(parameter));
        }

        /// <summary>
        /// Checks if the command is requested to be cancelled.
        /// </summary>
        public bool IsCancellationRequested => _cts?.IsCancellationRequested ?? false;

        public void Execute(object parameter)
        {
            ExecuteAsync(parameter).RunWithExceptionHandling(ex => _onException?.Invoke(ex), _continueOnCapturedContext);
        }

        public global::System.Threading.Tasks.Task ExecuteAsync(object? parameter)
        {
            return ExecuteAsync((T?)parameter);
        }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The command parameter of type <typeparamref name="T"/>.</param>
        public async global::System.Threading.Tasks.Task ExecuteAsync(T? parameter)
        {
            if (_execute == null)
                throw new global::System.InvalidOperationException("No action has been specified for the command.");

            if (CanExecute(parameter))
            {
                _cts = new global::System.Threading.CancellationTokenSource();
                IsRunning = true;
                Progress = 0;

                try
                {
                    await _execute(parameter, _cts.Token);
                }
                finally
                {
                    Progress = 0;
                    IsRunning = false;
                    _cts.Dispose();
                    _cts = null;
                }
            }
        }

        /// <summary>
        /// Cancels the currently running command.
        /// </summary>
        public void Cancel()
        {
            if (IsRunning && _cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Do(global::System.Func<T?, global::System.Threading.Tasks.Task> execute, IFluentSetterViewModel? owner)
        {
            var command = new AsyncCommand<T>
            {
                Owner = owner
            };
            command.SetExecute((o, _) => execute(o));
            return command;
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand{T}"/>.</returns>
        public static AsyncCommand<T> Do(global::System.Func<T?, global::System.Threading.CancellationToken, System.Threading.Tasks.Task> execute, IFluentSetterViewModel? owner)
        {
            var command = new AsyncCommand<T>
            {
                Owner = owner
            };
            command.SetExecute(execute);
            return command;
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute with a parameter.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand<T> If(global::System.Func<T?, bool> canExecute)
        {
            if (IsBuilt)
                return this;

            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Specifies an action to handle exceptions during execution.
        /// </summary>
        /// <param name="onException">The action to handle exceptions.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand<T> OnException(global::System.Action<global::System.Exception> onException)
        {
            if (IsBuilt)
                return this;

            _onException = onException;
            return this;
        }

        /// <summary>
        /// Sets whether the execution should continue on the captured synchronization context.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to continue on captured context.</param>
        /// <returns>The updated command instance.</returns>
        public AsyncCommand<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            if (IsBuilt)
                return this;

            _continueOnCapturedContext = continueOnCapturedContext;
            return this;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null.");

            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);

        /// <summary>
        /// Releases resources used by the command.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the command and any related resources.
        /// </summary>
        /// <param name="disposing">Whether the method is called from <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _execute = null;
                    _canExecute = null;
                    _onException = null;
                    _continueOnCapturedContext = false;

                    CanExecuteChanged = null;
                    PropertyChanged = null;
                    _cts?.Dispose();
                    _cancelCommand?.Dispose();
                    _cancelCommand = null;
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for the <see cref="AsyncCommand"/> class.
        /// </summary>
        ~AsyncCommand()
        {
            Dispose(false);
        }
    }
}