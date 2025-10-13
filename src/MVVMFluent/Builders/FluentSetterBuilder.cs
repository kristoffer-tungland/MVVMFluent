using MVVMFluent.Interfaces;
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MVVMFluent.Builders;

/// <summary>
/// Represents a builder for a fluent setter, see <see cref="FluentSetter{TValue}"/>.
/// </summary>
public class FluentSetterBuilder<TValue> : FluentSetterBuilderBase<TValue>, IFluentSetterBuilder, IFluentSetter<TValue>
{
    private readonly FluentSetter<TValue> _fluentSetter;

    public FluentSetterBuilder(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel, [CallerMemberName] string? propertyName = null)
        : base(valueToSet, fluentSetterViewModel)
    {
        _fluentSetter = CreateFluentSetter(fluentSetterViewModel, propertyName);
    }

    protected override IFluentSetter<TValue> FluentSetter => _fluentSetter;

    protected virtual FluentSetter<TValue> CreateFluentSetter(IFluentSetterViewModel fluentSetterViewModel, string? propertyName)
    {
        return new FluentSetter<TValue>(fluentSetterViewModel, propertyName);
    }

    protected FluentSetter<TValue> FluentSetterInstance => _fluentSetter;

    public string PropertyName => FluentSetterInstance.PropertyName;

    public IFluentSetter<TValue> Changing(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Changing(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Changing(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Changed(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Changed(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Changed(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public IFluentSetter<TValue> Notify(params ICommand[] commands)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(commands);
        }

        return this;
    }

    public IFluentSetter<TValue> Notify(params string[] propertyNames)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(propertyNames);
        }

        return this;
    }

    public override void Set()
    {
        FluentSetterInstance.SetValue(_valueToSet);
        base.Set();
    }
}
