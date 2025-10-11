using System;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Represents a builder for a fluent setter, see <see cref="FluentSetter{TValue}"/>.
/// </summary>
public class FluentSetterBuilder<TValue> : FluentSetterBuilderBase<TValue>, IFluentSetterBuilder
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

    public FluentSetterBuilder<TValue> Changing(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Changing(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Changing(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Changed(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Changed(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Changed(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Notify(params IFluentCommand[] commands)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(commands);
        }

        return this;
    }

    public FluentSetterBuilder<TValue> Notify(params string[] propertyNames)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(propertyNames);
        }

        return this;
    }
}
