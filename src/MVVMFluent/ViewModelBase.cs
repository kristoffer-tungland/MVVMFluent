namespace MVVMFluent
{
    /// <summary>
    /// Represents a base class for view models that provides property change notification and command creation.
    /// <example>
    /// <code lang="csharp">
    /// internal class MainViewModel : ViewModelBase
    /// {
    ///     // Property with notification and default value
    ///     public bool Enable { get => Get(true); set => Set(value); }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    internal abstract partial class ViewModelBase : global::System.ComponentModel.INotifyPropertyChanged, global::System.IDisposable
    {
        private readonly global::System.Collections.Generic.Dictionary<string, object?> _fieldStore = new global::System.Collections.Generic.Dictionary<string, object?>();
        private readonly global::System.Collections.Generic.Dictionary<string, IFluentCommand> _commandStore = new global::System.Collections.Generic.Dictionary<string, IFluentCommand>();

        private bool _disposed = false;

        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets the value of a property and notifies that the property has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        protected internal void Set<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            var setter = new FluentSetter<T>(this, propertyName, _fieldStore);
            setter.When(value).Set();
        }

        /// <summary>
        /// Creates a fluent setter for a property.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <returns>The fluent setter instance.</returns>
        protected internal FluentSetter<T> When<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            var setter = new FluentSetter<T>(this, propertyName, _fieldStore);
            return setter.When(value);
        }

        /// <summary>
        /// Gets the value of a property, returning a default value if not set.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="defaultValue">The default value to return if the property is not set.</param>
        /// <param name="propertyName">The name of the property being retrieved.</param>
        /// <returns>The value of the property.</returns>
        protected internal T? Get<T>(T? defaultValue = default, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to get.");

            if (_fieldStore.TryGetValue(propertyName, out var value))
            {
                return (T?)value;
            }

            _fieldStore.Add(propertyName, defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// Creates a command that can execute the specified action.
        /// <example>
        /// <code lang="csharp">
        /// public Command OkCommand => Do(() => MessageBox.Show("OK")).If(() => CanOk);
        /// 
        /// public bool CanOk { get => Get(true); set => Set(value); }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A command instance.</returns>
        protected internal Command Do(global::System.Action execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do(_ => execute(), propertyName);
        }

        /// <summary>
        /// Creates a command that can execute the specified action with a parameter.
        /// <example>
        /// <code lang="csharp">
        /// public Command OkCommand => Do&lt;string&gt;(str => MessageBox.Show(str)).If(str => CanOk(str));
        /// 
        /// public bool CanOk(string str) => !string.IsNullOrWhiteSpace(str);
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the command.</typeparam>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A command instance.</returns>
        protected internal Command<T> Do<T>(global::System.Action<T> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command<T>.Do(execute);
                _commandStore.Add(propertyName, command);
            }
            return (Command<T>)command;
        }

        
        protected internal Command Do(global::System.Action<object?> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command.Do(execute);
                _commandStore.Add(propertyName, command);
            }
            return (Command)command;
        }

        /// <summary>
        /// Creates an asynchronous command that supports cancellation and tracks execution state.
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
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>An asynchronous command instance.</returns>
        protected internal AsyncCommand Do(global::System.Func<global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do(_ => execute(), propertyName);
        }

        protected internal AsyncCommand<T> Do<T>(global::System.Func<T?, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = AsyncCommand<T>.Do(execute);
                _commandStore.Add(propertyName, command);
            }
            return (AsyncCommand<T>)command;
        }

        protected internal AsyncCommand Do(global::System.Func<object?, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = AsyncCommand.Do(execute);
                _commandStore.Add(propertyName, command);
            }

            return (AsyncCommand)command;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        internal void OnPropertyChanged([global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to notify.");

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

        protected void Dispose(bool disposing)
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
                _fieldStore.Clear();

                DisposeInternal();
            }

            _disposed = true;
        }

        protected virtual void DisposeInternal() {}

        /// <summary>
        /// Finalizer for the <see cref="ViewModelBase"/> class.
        /// </summary>
        ~ViewModelBase()
        {
            Dispose(false);
        }
    }
}