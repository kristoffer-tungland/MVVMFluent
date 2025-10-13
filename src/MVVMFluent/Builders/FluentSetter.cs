using MVVMFluent.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace MVVMFluent.Builders;

/// <summary>
/// Provides fluent configuration for setting property values and invoking change notifications.
/// </summary>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public class FluentSetter<TValue> : IFluentSetter<TValue>, IDisposable
{
    private readonly IFluentSetterViewModel _viewModel;
    private Action<TValue?>? _onChanging;
    private Action<TValue?, TValue?>? _onChangingOldNew;
    private Action<TValue?>? _onChanged;
    private Action<TValue?, TValue?>? _onChangedOldNew;
    private IEnumerable<ICommand>? _commandsToReevaluate;
    private IEnumerable<string>? _propertiesToNotify;
    private TValue? _valueToSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSetter{TValue}"/> class.
    /// </summary>
    /// <param name="viewModel">The owning view model that stores property values.</param>
    /// <param name="propertyName">The name of the property to update.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="viewModel"/> or <paramref name="propertyName"/> is <see langword="null"/>.</exception>
    public FluentSetter(IFluentSetterViewModel viewModel, string? propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (viewModel == null)
        {
            throw new ArgumentNullException(nameof(viewModel));
        }

        _viewModel = viewModel;
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the name of the property that this setter updates.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the value that will be assigned to the property when <see cref="Set"/> is called.
    /// </summary>
    protected TValue? ValueToSet => _valueToSet;

    internal void SetValue(TValue? value) => _valueToSet = value;

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanging = _ => action();
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action<TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanging = action;
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action<TValue?, TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChangingOldNew = action;
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanged = _ => action();
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action<TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanged = action;
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action<TValue?, TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChangedOldNew = action;
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Notify(params ICommand[] commands)
    {
        _commandsToReevaluate = commands;
        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Notify(params string[] propertyNames)
    {
        _propertiesToNotify = propertyNames;
        return this;
    }

    /// <inheritdoc />
    public virtual void Set()
    {
        var oldValue = _viewModel.GetFieldValue<TValue>(PropertyName);
        var hasChanged = !EqualityComparer<TValue?>.Default.Equals(oldValue, _valueToSet);

        if (!hasChanged)
        {
            return;
        }

        _onChanging?.Invoke(_valueToSet);
        _onChangingOldNew?.Invoke(oldValue, _valueToSet);

        _viewModel.SetFieldValue(PropertyName, _valueToSet);

        _onChanged?.Invoke(_valueToSet);
        _onChangedOldNew?.Invoke(oldValue, _valueToSet);

        if (_commandsToReevaluate != null)
        {
            foreach (var command in _commandsToReevaluate)
            {
                if (command is IFluentCommand fluentCommand)
                {
                    fluentCommand.RaiseCanExecuteChanged();
                    continue;
                }

                if (command is System.Windows.Input.ICommand cmd)
                {
                    var eventField = cmd.GetType().GetField("CanExecuteChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (eventField != null)
                    {
                        var eventDelegate = eventField.GetValue(cmd) as System.MulticastDelegate;
                        if (eventDelegate != null)
                        {
                            foreach (var handler in eventDelegate.GetInvocationList())
                            {
                                handler.Method.Invoke(handler.Target, new object[] { cmd, EventArgs.Empty });
                            }
                        }
                    }
                }
            }
        }

        if (_propertiesToNotify != null)
        {
            foreach (var propertyName in _propertiesToNotify)
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    _viewModel.OnPropertyChanged(propertyName);
                }
            }
        }
    }

    /// <summary>
    /// Releases the resources used by the fluent setter.
    /// </summary>
    public virtual void Dispose()
    {
        _onChanging = null;
        _onChangingOldNew = null;
        _onChanged = null;
        _onChangedOldNew = null;
        _commandsToReevaluate = null;
        _propertiesToNotify = null;
    }
}
