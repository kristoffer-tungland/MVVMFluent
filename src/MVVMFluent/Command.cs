namespace MVVMFluent
{
    internal interface IFluentCommand : global::System.Windows.Input.ICommand, global::System.IDisposable
    {
        void RaiseCanExecuteChanged();
    }


    /// <summary>
    /// Represents a command that can be executed and has an associated execution condition.
    /// </summary>
    internal class Command : IFluentCommand, global::System.IDisposable
    {
        private global::System.Action<object?>? _execute;
        private global::System.Func<object, bool>? _canExecute;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class with the specified execute action.
        /// </summary>
        protected Command() {}

        /// <summary>
        /// Occurs when the ability to execute the command changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <returns>A new <see cref="Command"/> instance.</returns>
        internal static Command Do(global::System.Action execute)
        {
            return Do(_ => execute());
        }

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <returns>A new <see cref="Command"/> instance.</returns>
        internal static Command Do(global::System.Action<object?> execute)
        {
            var command = new Command
            {
                _execute = execute
            };

            return command;
        }

        /// <summary>
        /// Sets the condition under which the command can execute.
        /// </summary>
        /// <param name="canExecute">A function that determines if the command can execute.</param>
        /// <returns>The current <see cref="Command"/> instance.</returns>
        internal Command If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Sets the condition under which the command can execute.
        /// </summary>
        /// <param name="canExecute">A function that determines if the command can execute based on the provided parameter.</param>
        /// <returns>The current <see cref="Command"/> instance.</returns>
        internal Command If(global::System.Func<object, bool> canExecute)
        {
            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Determines whether the command can be executed with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to check.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the execute action.</param>
        public void Execute(object parameter)
        {
            if (_execute == null)
                throw new global::System.InvalidOperationException("No execute action has been set for this command.");

            if (CanExecute(parameter))
                _execute?.Invoke(parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);

        /// <summary>
        /// Releases all resources used by the <see cref="Command"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and optionally releases managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected internal virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Unsubscribe from CanExecuteChanged event
                CanExecuteChanged = null;

                // Additional cleanup (if needed)
                _execute = null;
                _canExecute = null;
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer for the <see cref="Command"/> class.
        /// </summary>
        ~Command()
        {
            Dispose(false);
        }
    }

    /// <summary>
    /// Represents a command that can be executed with a parameter of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the parameter used by the command.</typeparam>
    internal class Command<T> : IFluentCommand, global::System.IDisposable
    {
        private global::System.Action<T>? _execute;
        private global::System.Func<T, bool>? _canExecute;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        protected Command() {}

        /// <summary>
        /// Occurs when the ability of the command to execute has changed.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;
        
        /// <summary>
        /// Creates a new command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <returns>A new instance of <see cref="Command{T}"/>.</returns>
        internal static Command<T> Do(global::System.Action<T> execute)
        {
            return new Command<T>
            {
                _execute = execute
            };
        }

        /// <summary>
        /// Configures the command to determine whether it can execute based on a condition.
        /// </summary>
        /// <param name="canExecute">A function that returns a boolean indicating whether the command can execute.</param>
        /// <returns>The current <see cref="Command{T}"/> instance for fluent chaining.</returns>
        internal Command<T> If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Configures the command to determine whether it can execute based on a condition that uses a parameter.
        /// </summary>
        /// <param name="canExecute">A function that determines whether the command can execute based on the provided parameter.</param>
        /// <returns>The current <see cref="Command{T}"/> instance for fluent chaining.</returns>
        internal Command<T> If(global::System.Func<T, bool> canExecute)
        {
            _canExecute = canExecute;
            return this;
        }

        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to check against the <see cref="_canExecute"/> condition.</param>
        /// <returns><c>true</c> if the command can execute; otherwise, <c>false</c>.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the command's execute action.</param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute?.Invoke((T)parameter);
        }

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event.
        /// </summary>
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, global::System.EventArgs.Empty);

        /// <summary>
        /// Releases all resources used by the <see cref="Command{T}"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Command{T}"/> instance and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected internal virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Additional cleanup (if needed)
                _execute = null;
                _canExecute = null;
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer for the <see cref="Command{T}"/> class.
        /// </summary>
        ~Command()
        {
            Dispose(false);
        }
    }
}