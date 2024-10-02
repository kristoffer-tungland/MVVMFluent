namespace MVVMFluent
{
    /// <summary>
    /// Represents a base class for view models that provides property change notification and command creation.
    /// <example>
    /// <code lang="csharp">
    /// public class MainViewModel : ViewModelBase
    /// {
    ///     // Property with notification and default value
    ///     public bool Enable { get => Get(true); set => Set(value); }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public abstract class FluentSetterViewModelBase : NotificationViewModelBase, IFluentSetterViewModel
    {
        protected readonly global::System.Collections.Generic.Dictionary<string, object?> _fieldStore = new();
        protected readonly global::System.Collections.Generic.Dictionary<string, IFluentCommand> _commandStore = new();
        protected readonly global::System.Collections.Generic.Dictionary<string, IFluentSetterBuilder> _builderStore = new();

        public void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder)
        {
            _builderStore.Add(fluentSetterBuilder.GetPropertyName(), fluentSetterBuilder);
        }

        public IFluentSetterBuilder? GetFluentSetterBuilder(string propertyName)
        {
            if (_builderStore.TryGetValue(propertyName, out var setter))
            {
                if (!setter.IsBuilt)
                    setter._intBuild();

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
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected void Set<TValue>(TValue value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Property name must be provided when it is set in the constructor.");

            SetFieldValue(propertyName, value);
        }

        /// <summary>
        /// Gets the value of a property, returning a default value if not set.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="defaultValue">The default value to return if the property is not set.</param>
        /// <param name="propertyName">The name of the property being retrieved.</param>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        /// <returns>The value of the property.</returns>
        protected TValue? Get<TValue>(TValue? defaultValue = default, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to get.");

            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Property name must be provided when it is retrieved in the constructor.");

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
        /// <returns>A new <see cref="Command"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected Command Do(global::System.Action execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
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
        /// <returns>Returns a new <see cref="Command{TValue}"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected Command<TValue> Do<TValue>(global::System.Action<TValue> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            // Error handling when property name is .ctor
            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Command name must be provided when it is created in the constructor.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = Command<TValue>.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

            return (Command<TValue>)command;
        }

        /// <summary>
        /// Creates a command that can execute the specified action with a parameter.
        /// <example>
        /// <code lang="csharp">
        /// public Command OkCommand => Do(obj => MessageBox.Show(obj.ToString()));
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>Use generic version for type safety.</remarks>
        /// <param name="execute">The action to execute. The parameter is of type object.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>Returns a new <see cref="Command"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected Command Do(global::System.Action<object?> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            // Error handling when property name is .ctor
            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Command name must be provided when it is created in the constructor.");

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
        /// public AsyncCommand OkCommand => Do(async () => await Task.Delay(1000)).If(() => CanOk).Handle(ex => MessageBox.Show(ex.Message)).ConfigureAwait(false);
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
        /// <returns>A new <see cref="AsyncCommand"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand Do(global::System.Func<global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do((object? _) => execute(), propertyName);
        }

        /// <summary>
        /// Creates an asynchronous command that passes a cancellation token to the method.
        /// <example>
        /// <code lang="csharp">
        /// public AsyncCommand OkCommand => Do(Ok);
        /// 
        /// private Task Ok(CancellationToken cancellationToken) { }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A new <see cref="AsyncCommand"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand Do(global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do((_,ct) => execute(ct), propertyName);
        }

        protected AsyncCommand<TValue> Do<TValue>(global::System.Func<global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do<TValue>((_,ct) => execute(ct), propertyName);
        }

        /// <summary>
        /// Creates an asynchronous command that supports cancellation and tracks execution state.
        /// <example>
        /// <code lang="csharp">
        /// // Short command
        /// public AsyncCommand OkCommand => Do(async () => await Task.Delay(1000)).If(() => CanOk);
        /// </code>
        /// <code lang="csharp">
        /// // Command with all options
        /// public AsyncCommand&lt;string&gt; OkCommand => Do&lt;string&gt;(str => Task.Delay(1000).ContinueWith(_ => MessageBox.Show(str))).If(str => CanOk(str)).Handle(ex => MessageBox.Show(ex.Message)).ConfigureAwait(false);
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TValue">The type of the parameter for the command.</typeparam>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A new <see cref="AsyncCommand{TValue}"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand<TValue> Do<TValue>(global::System.Func<TValue?, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do<TValue>((para, ct) => execute(para), propertyName);
        }

        /// <summary>
        /// Creates an asynchronous command that passes a cancellation token to the method.
        /// <example>
        /// <code lang="csharp">
        /// public AsyncCommand&lt;string&gt; OkCommand => Do&lt;string?&gt;(Ok);
        /// 
        /// private Task Ok(string? str, CancellationToken cancellationToken) { }
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TValue">The type of the parameter for the command.</typeparam>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A new <see cref="AsyncCommand{TValue}"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand<TValue> Do<TValue>(global::System.Func<TValue?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            // Error handling when property name is .ctor
            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Command name must be provided when it is created in the constructor.");

            if (!_commandStore.TryGetValue(propertyName, out var command))
            {
                command = AsyncCommand<TValue>.Do(execute, this);
                _commandStore.Add(propertyName, command);
            }
            else
                command.MarkAsBuilt();

            return (AsyncCommand<TValue>)command;
        }

        /// <summary>
        /// Creates an asynchronous command that supports cancellation and tracks execution state.
        /// <example>
        /// <code lang="csharp">
        /// public AsyncCommand OkCommand => Do(async obj => await Task.Delay(1000)).If(obj => CanOk(obj));
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>Use generic version for type safety.</remarks>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A new <see cref="AsyncCommand"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand Do(global::System.Func<object?, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            return Do((para, ct) => execute(para), propertyName);
        }

        /// <summary>
        /// Creates an asynchronous command that passes a cancellation token to the method.
        /// <example>
        /// <code lang="csharp">
        /// public AsyncCommand OkCommand => Do(Ok);
        /// 
        /// private Task Ok(CancellationToken cancellationToken) { }
        /// </code>
        /// </example>
        /// </summary>
        /// <remarks>Use generic version for type safety.</remarks>
        /// <param name="execute">The action to execute.</param>
        /// <param name="propertyName">The name of the property associated with the command.</param>
        /// <returns>A new <see cref="AsyncCommand"/> instance.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property name is .ctor.</exception>
        protected AsyncCommand Do(global::System.Func<object?, global::System.Threading.CancellationToken, global::System.Threading.Tasks.Task> execute, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            // Error handling when property name is .ctor
            if (propertyName == ".ctor")
                throw new global::System.ArgumentException(nameof(propertyName), "Command name must be provided when it is created in the constructor.");

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
                if (command is global::System.IDisposable disposableCommand)
                {
                    disposableCommand.Dispose();
                }
            }

            _commandStore.Clear();

            foreach (var builder in _builderStore.Values)
            {
                if (builder is global::System.IDisposable disposableBuilder)
                {
                    disposableBuilder.Dispose();
                }
            }
            _builderStore.Clear();

            foreach (var field in _fieldStore.Values)
            {
                if (field is global::System.IDisposable disposableField)
                {
                    disposableField.Dispose();
                }
            }

            _fieldStore.Clear();

            base.DisposeInternal();
        }
    }
}