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
    public abstract class FluentSetterViewModelBase : NotificationViewModelBase, IFluentSetterViewModel
    {
        protected readonly System.Collections.Generic.Dictionary<string, object?> _fieldStore = new System.Collections.Generic.Dictionary<string, object?>();
        protected readonly System.Collections.Generic.Dictionary<string, IFluentCommand> _commandStore = new System.Collections.Generic.Dictionary<string, IFluentCommand>();
        protected readonly System.Collections.Generic.Dictionary<string, IFluentSetterBuilder> _builderStore = new System.Collections.Generic.Dictionary<string, IFluentSetterBuilder>();

        public void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder)
        {
            _builderStore.Add(fluentSetterBuilder.GetPropertyName(), fluentSetterBuilder);
        }

        public IFluentSetterBuilder? GetFluentSetterBuilder(string propertyName)
        {
            if (_builderStore.TryGetValue(propertyName, out var setter))
            {
                if (!setter.IsBuilt)
                    setter.Build();

                return setter;
            }

            return null;
        }

        public TValue? GetFieldValue<TValue>(string propertyName)
        {
            if (_fieldStore.TryGetValue(propertyName, out var value))
            {
                return (TValue?)value;
            }
            return default;
        }

        public void SetFieldValue<TValue>(string propertyName, TValue? value)
        {
            _fieldStore[propertyName] = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        /// Sets the value of a property and notifies that the property has changed.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        protected void Set<TValue>(TValue value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            SetFieldValue(propertyName, value);
        }

        /// <summary>
        /// Gets the value of a property, returning a default value if not set.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="defaultValue">The default value to return if the property is not set.</param>
        /// <param name="propertyName">The name of the property being retrieved.</param>
        /// <returns>The value of the property.</returns>
        protected TValue? Get<TValue>(TValue? defaultValue = default, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to get.");

            if (_fieldStore.TryGetValue(propertyName, out var value))
                return (TValue?)value;

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
        protected Command Do(System.Action execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
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
        /// <typeparam name="TValue">The type of the parameter for the command.</typeparam>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A command instance.</returns>
        protected Command<TValue> Do<TValue>(System.Action<TValue> execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command<TValue>.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

            return (Command<TValue>)command;
        }


        protected Command Do(System.Action<object?> execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

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
        protected AsyncCommand Do(System.Func<System.Threading.Tasks.Task> execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do(_ => execute(), propertyName);
        }

        protected AsyncCommand<TValue> Do<TValue>(System.Func<TValue?, System.Threading.Tasks.Task> execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = AsyncCommand<TValue>.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

            return (AsyncCommand<TValue>)command;
        }

        protected AsyncCommand Do(System.Func<object?, System.Threading.Tasks.Task> execute, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = AsyncCommand.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

            return (AsyncCommand)command;
        }

        protected override void DisposeInternal()
        {
            // Clean up commands
            foreach (var command in _commandStore.Values)
            {
                if (command is System.IDisposable disposableCommand)
                {
                    disposableCommand.Dispose();
                }
            }

            _commandStore.Clear();

            foreach (var builder in _builderStore.Values)
            {
                if (builder is System.IDisposable disposableBuilder)
                {
                    disposableBuilder.Dispose();
                }
            }
            _builderStore.Clear();

            foreach (var field in _fieldStore.Values)
            {
                if (field is System.IDisposable disposableField)
                {
                    disposableField.Dispose();
                }
            }

            _fieldStore.Clear();

            base.DisposeInternal();
        }
    }
}