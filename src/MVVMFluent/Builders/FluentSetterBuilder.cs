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

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSetterBuilder{TValue}"/> class.
    /// </summary>
    /// <param name="valueToSet">The value to assign to the property.</param>
    /// <param name="fluentSetterViewModel">The owning view model that manages fluent setters.</param>
    /// <param name="propertyName">The name of the property to update. Automatically provided by the compiler when omitted.</param>
    public FluentSetterBuilder(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel, [CallerMemberName] string? propertyName = null)
        : base(valueToSet, fluentSetterViewModel)
    {
        _fluentSetter = CreateFluentSetter(fluentSetterViewModel, propertyName);
    }

    /// <inheritdoc />
    protected override IFluentSetter<TValue> FluentSetter => _fluentSetter;

    /// <summary>
    /// Creates the underlying <see cref="FluentSetter{TValue}"/> instance.
    /// </summary>
    /// <param name="fluentSetterViewModel">The owning view model.</param>
    /// <param name="propertyName">The name of the property being configured.</param>
    /// <returns>A new <see cref="FluentSetter{TValue}"/>.</returns>
    protected virtual FluentSetter<TValue> CreateFluentSetter(IFluentSetterViewModel fluentSetterViewModel, string? propertyName)
    {
        return new FluentSetter<TValue>(fluentSetterViewModel, propertyName);
    }

    /// <summary>
    /// Gets the underlying <see cref="FluentSetter{TValue}"/> instance used to configure notifications and callbacks.
    /// </summary>
    protected FluentSetter<TValue> FluentSetterInstance => _fluentSetter;

    /// <summary>
    /// Gets the name of the property that this builder targets.
    /// </summary>
    public string PropertyName => FluentSetterInstance.PropertyName;

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changing(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changing(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action<TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Changed(Action<TValue?, TValue?> action)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Changed(action);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Notify(params ICommand[] commands)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(commands);
        }

        return this;
    }

    /// <inheritdoc />
    public IFluentSetter<TValue> Notify(params string[] propertyNames)
    {
        if (!IsBuilt)
        {
            FluentSetterInstance.Notify(propertyNames);
        }

        return this;
    }

    /// <inheritdoc />
    public override void Set()
    {
        FluentSetterInstance.SetValue(_valueToSet);
        base.Set();
    }
}
