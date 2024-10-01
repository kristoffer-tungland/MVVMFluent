using global::System;

namespace MVVMFluent
{
    public interface IFluentCommand : global::System.Windows.Input.ICommand, global::System.IDisposable
    {
        void MarkAsBuilt();
        bool IsBuilt { get; }
        IFluentSetterViewModel? Owner { get; }
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Represents a command that can be executed and has an associated execution condition.
    /// <example>
    /// <code lang="csharp">
    /// public Command OkCommand => Do(() => MessageBox.Show("OK")).If(() => CanOk);
    /// 
    /// public bool CanOk { get => Get(true); set => Set(value); }
    /// </code>
    /// </example>
    /// </summary>
    public class Command : IFluentCommand, global::System.IDisposable
    {
        private global::System.Action<object?>? _execute;
        private global::System.Func<object, bool>? _canExecute;
        private bool _disposed = false;

        public bool IsBuilt { get; private set; }

        public IFluentSetterViewModel? Owner { get; private set; }

        public void MarkAsBuilt()
        {
            IsBuilt = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class with the specified execute action.
        /// </summary>
        protected void SetCommand(global::System.Action<object?> execute)
        {
            _execute = execute;
        }

        /// <summary>
        /// Occurs when the ability to execute the command changes.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <returns>A new <see cref="Command"/> instance.</returns>
        public static Command Do(global::System.Action execute, IFluentSetterViewModel? owner)
        {
            return Do(_ => execute(), owner);
        }

        /// <summary>
        /// Creates a new <see cref="Command"/> instance with the specified action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <returns>A new <see cref="Command"/> instance.</returns>
        public static Command Do(global::System.Action<object?> execute, IFluentSetterViewModel? owner)
        {
            var command = new Command();
            command.Owner = owner;
            command.SetCommand(execute);
            return command;
        }

        /// <summary>
        /// Sets the condition under which the command can execute.
        /// </summary>
        /// <param name="canExecute">A function that determines if the command can execute.</param>
        /// <returns>The current <see cref="Command"/> instance.</returns>
        public Command If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Sets the condition under which the command can execute.
        /// </summary>
        /// <param name="canExecute">A function that determines if the command can execute based on the provided parameter.</param>
        /// <returns>The current <see cref="Command"/> instance.</returns>
        public Command If(global::System.Func<object, bool> canExecute)
        {
            if (IsBuilt)
                return this;

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
        protected virtual void Dispose(bool disposing)
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
    /// <example>
    /// <code lang="csharp">
    /// public Command OkCommand => Do&lt;string&gt;(str => MessageBox.Show(str)).If(str => CanOk(str));
    /// 
    /// public bool CanOk(string str) => !string.IsNullOrWhiteSpace(str);
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T">The type of the parameter used by the command.</typeparam>
    public class Command<T> : IFluentCommand, global::System.IDisposable
    {
        private global::System.Action<T>? _execute;
        private global::System.Func<T, bool>? _canExecute;
        private bool _disposed = false;
        public bool IsBuilt { get; private set; }
        public IFluentSetterViewModel? Owner { get; private set; }

        public void MarkAsBuilt()
        {
            IsBuilt = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command{T}"/> class with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        protected void SetCommand(global::System.Action<T> execute)
        {
            _execute = execute;
        }

        /// <summary>
        /// Occurs when the ability of the command to execute has changed.
        /// </summary>
        public event global::System.EventHandler? CanExecuteChanged;

        /// <summary>
        /// Creates a new command with the specified execute action.
        /// </summary>
        /// <param name="execute">The action to execute when the command is invoked.</param>
        /// <returns>A new instance of <see cref="Command{T}"/>.</returns>
        public static Command<T> Do(global::System.Action<T> execute, IFluentSetterViewModel? owner)
        {
            var command = new Command<T>
            {
                Owner = owner
            };
            command.SetCommand(execute);
            return command;
        }

        /// <summary>
        /// Configures the command to determine whether it can execute based on a condition.
        /// </summary>
        /// <param name="canExecute">A function that returns a boolean indicating whether the command can execute.</param>
        /// <returns>The current <see cref="Command{T}"/> instance for fluent chaining.</returns>
        public Command<T> If(global::System.Func<bool> canExecute)
        {
            return If(_ => canExecute());
        }

        /// <summary>
        /// Configures the command to determine whether it can execute based on a condition that uses a parameter.
        /// </summary>
        /// <param name="canExecute">A function that determines whether the command can execute based on the provided parameter.</param>
        /// <returns>The current <see cref="Command{T}"/> instance for fluent chaining.</returns>
        public Command<T> If(global::System.Func<T, bool> canExecute)
        {
            if (IsBuilt)
                return this;

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
        protected virtual void Dispose(bool disposing)
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
        /// Finalizer for the <see cref="Command{T}"/> class.
        /// </summary>
        ~Command()
        {
            Dispose(false);
        }
    }
}