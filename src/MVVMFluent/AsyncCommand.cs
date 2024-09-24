using System.Threading.Tasks;

namespace MVVMFluent
{
    /// <summary>
    /// Defines an asynchronous fluent command that supports cancellation and tracking of execution state.
    /// </summary>
    internal interface IAsyncFluentCommand : IFluentCommand, global::System.IDisposable
    {
        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        bool IsRunning { get; }

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
    /// </summary>
    internal class AsyncCommand : IAsyncFluentCommand, global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private global::System.Func<object?, global::System.Threading.Tasks.Task>? _execute;
        private global::System.Func<object?, bool>? _canExecute;
        private bool _isRunning;
        private global::System.Threading.CancellationTokenSource? _cts;
        private global::System.Action<global::System.Exception>? _onException;
        private bool _continueOnCapturedContext = true;
        private bool _disposed;

        /// <summary>
        /// Event raised when the ability to execute changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        private Command? _cancelCommand;

        public Command CandelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = Command.Do(() => Cancel()).If(() => IsRunning);
                    PropertyChanged += (s, e) => _cancelCommand.RaiseCanExecuteChanged();
                }
                return _cancelCommand;
            }
        }

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

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
                IsRunning = true;
                _cts = new global::System.Threading.CancellationTokenSource();

                try
                {
                    await _execute(parameter);
                }
                finally
                {
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
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand"/>.</returns>
        internal static AsyncCommand Do(global::System.Func<global::System.Threading.Tasks.Task> execute)
        {
            return Do(_ => execute());
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand"/>.</returns>
        internal static AsyncCommand Do(global::System.Func<object?, global::System.Threading.Tasks.Task> execute)
        {
            var command = new AsyncCommand
            {
                _execute = execute
            };
            return command;
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute with a parameter.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand If(global::System.Func<object?, bool> canExecute)
        {
            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Specifies an action to handle exceptions during execution.
        /// </summary>
        /// <param name="onException">The action to handle exceptions.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand OnException(global::System.Action<global::System.Exception> onException)
        {
            _onException = onException;
            return this;
        }

        /// <summary>
        /// Sets whether the execution should continue on the captured synchronization context.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to continue on captured context.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand ConfigureAwait(bool continueOnCapturedContext)
        {
            _continueOnCapturedContext = continueOnCapturedContext;
            return this;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        internal void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null.");

            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

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
                    _cts?.Dispose();
                    _execute = null;
                    _canExecute = null;
                    _onException = null;
                    _continueOnCapturedContext = false;
                    PropertyChanged = null;
                    CanExecuteChanged = null;
                }
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Defines an asynchronous fluent command that supports cancellation and tracking of execution state with a generic parameter.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    internal interface IAsyncFluentCommand<T> : IFluentCommand, global::System.IDisposable
    {
        /// <summary>
        /// Indicates whether the command is currently executing.
        /// </summary>
        bool IsRunning { get; }

        bool IsCancellationRequested { get; }

        /// <summary>
        /// Provides the cancellation token source used to cancel the current operation.
        /// </summary>
        global::System.Threading.CancellationTokenSource? CancellationTokenSource { get; }

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="parameter">The parameter of type <typeparamref name="T"/> for the command execution.</param>
        global::System.Threading.Tasks.Task ExecuteAsync(T parameter);

        /// <summary>
        /// Cancels the current operation if it is running.
        /// </summary>
        void Cancel();
    }

    /// <summary>
    /// Represents an asynchronous command that supports cancellation and tracks execution state, with a generic parameter.
    /// </summary>
    internal class AsyncCommand<T> : IAsyncFluentCommand, global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private global::System.Func<T?, global::System.Threading.Tasks.Task>? _execute;
        private global::System.Func<T?, bool>? _canExecute;
        private bool _isRunning;
        private global::System.Threading.CancellationTokenSource? _cts;
        private global::System.Action<global::System.Exception>? _onException;
        private bool _continueOnCapturedContext = true;
        private bool _disposed;

        /// <summary>
        /// Event raised when the ability to execute changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Event raised when a property changes.
        /// </summary>
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

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

        public Task ExecuteAsync(object? parameter)
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
                IsRunning = true;
                _cts = new global::System.Threading.CancellationTokenSource();

                try
                {
                    await _execute(parameter);
                }
                finally
                {
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
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);
        }

        /// <summary>
        /// Creates an asynchronous command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute asynchronously.</param>
        /// <returns>An instance of <see cref="AsyncCommand{T}"/>.</returns>
        internal static AsyncCommand<T> Do(global::System.Func<T?, global::System.Threading.Tasks.Task> execute)
        {
            var command = new AsyncCommand<T>
            {
                _execute = execute
            };
            return command;
        }

        /// <summary>
        /// Adds a condition to determine whether the command can execute with a parameter.
        /// </summary>
        /// <param name="canExecute">The condition function.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand<T> If(global::System.Func<T?, bool> canExecute)
        {
            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Specifies an action to handle exceptions during execution.
        /// </summary>
        /// <param name="onException">The action to handle exceptions.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand<T> OnException(global::System.Action<global::System.Exception> onException)
        {
            _onException = onException;
            return this;
        }

        /// <summary>
        /// Sets whether the execution should continue on the captured synchronization context.
        /// </summary>
        /// <param name="continueOnCapturedContext">Whether to continue on captured context.</param>
        /// <returns>The updated command instance.</returns>
        internal AsyncCommand<T> ConfigureAwait(bool continueOnCapturedContext)
        {
            _continueOnCapturedContext = continueOnCapturedContext;
            return this;
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        internal void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null.");

            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

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
                    _cts?.Dispose();
                    _execute = null;
                    _canExecute = null;
                    _onException = null;
                    _continueOnCapturedContext = false;
                    PropertyChanged = null;
                    CanExecuteChanged = null;
                }
                _disposed = true;
            }
        }
    }
}