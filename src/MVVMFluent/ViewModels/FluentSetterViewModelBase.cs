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
    protected readonly Dictionary<string, object?> _fieldStore = new();
    protected readonly Dictionary<string, IFluentCommand> _commandStore = new();
    protected readonly Dictionary<string, IFluentSetterBuilder> _builderStore = new();

    public void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder)
    {
        if (fluentSetterBuilder == null)
        {
            throw new ArgumentNullException(nameof(fluentSetterBuilder));
        }

        _builderStore[fluentSetterBuilder.GetPropertyName()] = fluentSetterBuilder;
    }

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
                builder._intBuild();
            }

            return builder;
        }

        return null;
    }

    public TValue? GetFieldValue<TValue>(string propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        return _fieldStore.TryGetValue(propertyName, out var value) ? (TValue?)value : default;
    }

    public void SetFieldValue<TValue>(string propertyName, TValue? value)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        _fieldStore[propertyName] = value;
        OnPropertyChanged(propertyName);
    }

    protected void Set<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        SetFieldValue(propertyName!, value);
    }

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

    protected FluentCommand Do(Action execute, [CallerMemberName] string? propertyName = null)
        => Do(_ => execute(), propertyName);

    protected FluentCommand Do(Action<object?> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => FluentCommand.Do(execute, this));
    }

    protected FluentCommand<TValue> Do<TValue>(Action<TValue> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => FluentCommand<TValue>.Do(value => execute(value ?? default(TValue)!), this));
    }

    protected AsyncFluentCommand Do(Func<Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((object? _) => execute(), propertyName);

    protected AsyncFluentCommand Do(Func<CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((_, token) => execute(token), propertyName);

    protected AsyncFluentCommand Do(Func<object?, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do((parameter, _) => execute(parameter), propertyName);

    protected AsyncFluentCommand Do(Func<object?, CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => AsyncFluentCommand.Do(execute, this));
    }

    protected AsyncFluentCommand<TValue> Do<TValue>(Func<Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((_, _) => execute(), propertyName);

    protected AsyncFluentCommand<TValue> Do<TValue>(Func<CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((_, token) => execute(token), propertyName);

    protected AsyncFluentCommand<TValue> Do<TValue>(Func<TValue?, Task> execute, [CallerMemberName] string? propertyName = null)
        => Do<TValue>((parameter, _) => execute(parameter), propertyName);

    protected AsyncFluentCommand<TValue> Do<TValue>(Func<TValue?, CancellationToken, Task> execute, [CallerMemberName] string? propertyName = null)
    {
        ValidatePropertyName(propertyName);
        return GetOrCreateCommand(propertyName!, () => AsyncFluentCommand<TValue>.Do(execute, this));
    }

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
