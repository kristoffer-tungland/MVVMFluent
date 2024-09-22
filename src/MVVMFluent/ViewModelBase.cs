namespace MVVMFluent
{
    internal abstract partial class ViewModelBase : global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private readonly global::System.Collections.Generic.Dictionary<string, object> _propertyStore = new global::System.Collections.Generic.Dictionary<string, object>();
        private readonly global::System.Collections.Generic.Dictionary<string, IFluentCommand> _commandStore = new global::System.Collections.Generic.Dictionary<string, IFluentCommand>();

        private bool _disposed = false;

        public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the value of a property and notifies that the property has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        protected internal void Set<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var setter = new FluentSetter<T>(this, propertyName, _propertyStore);
            setter.When(value).Set();
        }

        /// <summary>
        /// Creates a fluent setter for a property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <returns>The fluent setter instance.</returns>
        protected internal FluentSetter<T> When<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            var setter = new FluentSetter<T>(this, propertyName, _propertyStore);
            return setter.When(value);
        }

        /// <summary>
        /// Gets the value of a property, returning a default value if not set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="defaultValue">The default value to return if the property is not set.</param>
        /// <param name="propertyName">The name of the property being retrieved.</param>
        /// <returns>The value of the property.</returns>
        protected internal T Get<T>(T defaultValue = default, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (_propertyStore.TryGetValue(propertyName, out var value))
            {
                return (T)value;
            }

            _propertyStore.Add(propertyName, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Creates a command that can execute the specified action.
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A command instance.</returns>
        protected internal Command Do(global::System.Action execute, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            return Do(_ => execute(), propertyName);
        }

        protected internal Command Do(global::System.Action<object> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command.Do(execute);
                _commandStore.Add(propertyName, command);
            }
            return (Command)command;
        }

        /// <summary>
        /// Creates a command that can execute the specified action with a parameter.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the command.</typeparam>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A command instance.</returns>
        protected internal Command<T> Do<T>(global::System.Action<T> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command<T>.Do(execute);
                _commandStore.Add(propertyName, command);
            }
            return (Command<T>)command;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        internal void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ViewModelBase"/> instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        protected internal virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Clean up commands
                foreach (var command in _commandStore.Values)
                {
                    if (command is global::System.IDisposable disposableCommand)
                    {
                        disposableCommand.Dispose();
                    }
                }

                _commandStore.Clear();
                _propertyStore.Clear();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer for the <see cref="ViewModelBase"/> class.
        /// </summary>
        ~ViewModelBase()
        {
            Dispose(false);
        }
    }
}