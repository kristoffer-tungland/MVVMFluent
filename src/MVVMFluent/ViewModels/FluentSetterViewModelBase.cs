using MVVMFluent.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that provides property change notification and command creation.
/// </summary>
public abstract class FluentSetterViewModelBase : NotificationViewModelBase, IFluentSetterViewModel
{
    /// <summary>
    /// Storage for property values.
    /// </summary>
    protected readonly Dictionary<string, object?> _fieldStore = new();
    
    /// <summary>
    /// Cache for commands created via Do methods.
    /// </summary>
    protected readonly Dictionary<string, IFluentCommand> _commandStore = new();
    
    /// <summary>
    /// Cache for fluent setter builders created via When methods.
    /// </summary>
    protected readonly Dictionary<string, IFluentSetterBuilder> _builderStore = new();

    /// <summary>
    /// Adds a fluent setter builder to the internal cache.
    /// </summary>
    /// <param name="fluentSetterBuilder">The builder to add.</param>
    public void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder)
    {
        if (fluentSetterBuilder == null)
        {
            throw new ArgumentNullException(nameof(fluentSetterBuilder));
        }

        _builderStore[fluentSetterBuilder.GetPropertyName()] = fluentSetterBuilder;
    }

    /// <summary>
    /// Retrieves a fluent setter builder from the cache, building it if necessary.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The cached builder, or null if not found.</returns>
    public IFluentSetterBuilder? GetFluentSetterBuilder(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        if (_builderStore.TryGetValue(propertyName, out var builder))
        {
            if (!builder.IsBuilt)
            {
                builder.Build();
            }

            return builder;
        }

        return null;
    }

    /// <summary>
    /// Gets the stored value for a property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The property value, or default if not found.</returns>
    public TValue? GetFieldValue<TValue>(string propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        return _fieldStore.TryGetValue(propertyName, out var value) ? (TValue?)value : default;
    }

    /// <summary>
    /// Sets the value for a property and raises PropertyChanged notification.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The new value.</param>
    public void SetFieldValue<TValue>(string propertyName, TValue? value)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        _fieldStore[propertyName] = value;
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Sets a property value directly without fluent configuration.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The property name. Automatically captured from caller member name.</param>
    protected void Set<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        SetFieldValue(propertyName!, value);
    }

    /// <summary>
    /// Gets a property value, initializing with the default value if not yet set.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="defaultValue">The default value to use if the property hasn't been set.</param>
    /// <param name="propertyName">The property name. Automatically captured from caller member name.</param>
    /// <returns>The property value.</returns>
    protected TValue? Get<TValue>(TValue? defaultValue = default, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);

        if (_fieldStore.TryGetValue(propertyName!, out var value))
        {
            return (TValue?)value;
        }

        _fieldStore.Add(propertyName!, defaultValue);
        return defaultValue;
    }

    /// <summary>
    /// Creates a cached synchronous command with no parameters.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="FluentCommand"/> that can be configured with additional conditions.</returns>
    protected FluentCommand Do(Action execute, [CallerMemberName] string? propertyName = null)
        => Do(_ => execute(), propertyName);

    /// <summary>
    /// Creates a cached synchronous command with an optional parameter.
    /// </summary>
    /// <param name="execute">The action to execute with the command parameter.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="FluentCommand"/> that can be configured with additional conditions.</returns>
    protected FluentCommand Do(Action<object?> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => FluentCommand.Do(execute, this));
    }

    /// <summary>
    /// Creates a cached synchronous command with a strongly-typed parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the command parameter.</typeparam>
    /// <param name="execute">The action to execute with the strongly-typed parameter.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="FluentCommand{TValue}"/> that can be configured with additional conditions.</returns>
    protected FluentCommand<TValue> Do<TValue>(Action<TValue> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => FluentCommand<TValue>.Do(value => execute(value ?? default(TValue)!), this));
    }

    /// <summary>
    /// Creates a cached asynchronous command with no parameters.
    /// </summary>
    /// <param name="execute">The async function to execute.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand Do(Func<Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((object? _) => execute(), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with a cancellation token.
    /// </summary>
    /// <param name="execute">The async function to execute with cancellation support.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand Do(Func<CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((_, token) => execute(token), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with an optional parameter.
    /// </summary>
    /// <param name="execute">The async function to execute with the command parameter.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand Do(Func<object?, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((parameter, _) => execute(parameter), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with an optional parameter and cancellation token.
    /// </summary>
    /// <param name="execute">The async function to execute with the command parameter and cancellation support.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand Do(Func<object?, CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => AsyncFluentCommand.Do(execute, this));
    }

    /// <summary>
    /// Creates a cached asynchronous command with a strongly-typed parameter and no parameters.
    /// </summary>
    /// <typeparam name="TValue">The type of the command parameter.</typeparam>
    /// <param name="execute">The async function to execute.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand{TValue}"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand<TValue> Do<TValue>(Func<Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((_, _) => execute(), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with a strongly-typed parameter and cancellation token.
    /// </summary>
    /// <typeparam name="TValue">The type of the command parameter.</typeparam>
    /// <param name="execute">The async function to execute with cancellation support.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand{TValue}"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand<TValue> Do<TValue>(Func<CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((_, token) => execute(token), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with a strongly-typed parameter.
    /// </summary>
    /// <typeparam name="TValue">The type of the command parameter.</typeparam>
    /// <param name="execute">The async function to execute with the strongly-typed parameter.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand{TValue}"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand<TValue> Do<TValue>(Func<TValue?, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((parameter, _) => execute(parameter), propertyName);

    /// <summary>
    /// Creates a cached asynchronous command with a strongly-typed parameter and cancellation token.
    /// </summary>
    /// <typeparam name="TValue">The type of the command parameter.</typeparam>
    /// <param name="execute">The async function to execute with the strongly-typed parameter and cancellation support.</param>
    /// <param name="propertyName">The property name for caching. Automatically captured from caller member name.</param>
    /// <returns>A cached <see cref="AsyncFluentCommand{TValue}"/> that can be configured with cancellation and error handling.</returns>
    protected AsyncFluentCommand<TValue> Do<TValue>(Func<TValue?, CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => AsyncFluentCommand<TValue>.Do(execute, this));
    }

    /// <summary>
    /// Disposes all cached commands, builders, and field values.
    /// </summary>
    protected override void DisposeInternal()
    {
        foreach (var command in _commandStore.Values)
        {
            (command as IDisposable)?.Dispose();
        }

        _commandStore.Clear();

        foreach (var builder in _builderStore.Values)
        {
            (builder as IDisposable)?.Dispose();
        }

        _builderStore.Clear();

        foreach (var field in _fieldStore.Values)
        {
            (field as IDisposable)?.Dispose();
        }

        _fieldStore.Clear();
    }

    private TCommand GetOrCreateCommand<TCommand>(string propertyName, Func<TCommand> factory)
        where TCommand : class, IFluentCommand
    {
        if (_commandStore.TryGetValue(propertyName, out var existing))
        {
            existing.MarkAsBuilt();
            return (TCommand)existing;
        }

        var command = factory();
        _commandStore[propertyName] = command;
        return command;
    }

    private static void ValidatePropertyName(string? propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName), "Not able to determine property name.");
        }

        if (propertyName == ".ctor")
        {
            throw new ArgumentException("Property name must be provided when it is used inside a constructor.", nameof(propertyName));
        }
    }
}
