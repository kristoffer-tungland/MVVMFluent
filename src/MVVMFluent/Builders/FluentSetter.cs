using System;
using System.Collections.Generic;

namespace MVVMFluent;

public class FluentSetter<TValue> : IFluentSetter<TValue>, IDisposable
{
    private readonly IFluentSetterViewModel _viewModel;
    private Action<TValue?>? _onChanging;
    private Action<TValue?, TValue?>? _onChangingOldNew;
    private Action<TValue?>? _onChanged;
    private Action<TValue?, TValue?>? _onChangedOldNew;
    private IEnumerable<IFluentCommand>? _commandsToReevaluate;
    private IEnumerable<string>? _propertiesToNotify;

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

    public string PropertyName { get; }

    internal FluentSetter<TValue> Changing(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanging = _ => action();
        return this;
    }

    internal FluentSetter<TValue> Changing(Action<TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanging = action;
        return this;
    }

    internal FluentSetter<TValue> Changing(Action<TValue?, TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChangingOldNew = action;
        return this;
    }

    internal FluentSetter<TValue> Changed(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanged = _ => action();
        return this;
    }

    internal FluentSetter<TValue> Changed(Action<TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChanged = action;
        return this;
    }

    internal FluentSetter<TValue> Changed(Action<TValue?, TValue?> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        _onChangedOldNew = action;
        return this;
    }

    internal FluentSetter<TValue> Notify(params IFluentCommand[] commands)
    {
        _commandsToReevaluate = commands;
        return this;
    }

    internal FluentSetter<TValue> Notify(params string[] propertyNames)
    {
        _propertiesToNotify = propertyNames;
        return this;
    }

    public virtual void Set(TValue? value)
    {
        var oldValue = _viewModel.GetFieldValue<TValue>(PropertyName);
        var hasChanged = !EqualityComparer<TValue?>.Default.Equals(oldValue, value);

        if (!hasChanged)
        {
            return;
        }

        _onChanging?.Invoke(value);
        _onChangingOldNew?.Invoke(oldValue, value);

        _viewModel.SetFieldValue(PropertyName, value);

        _onChanged?.Invoke(value);
        _onChangedOldNew?.Invoke(oldValue, value);

        if (_commandsToReevaluate != null)
        {
            foreach (var command in _commandsToReevaluate)
            {
                command?.RaiseCanExecuteChanged();
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
