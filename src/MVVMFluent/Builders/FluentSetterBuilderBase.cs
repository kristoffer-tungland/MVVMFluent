using MVVMFluent.Interfaces;
using System;

namespace MVVMFluent.Builders;

/// <summary>
/// Base class for fluent setter builders.
/// </summary>
/// <typeparam name="TValue">The type of the value the FluentSetter will be used with.</typeparam>
public abstract class FluentSetterBuilderBase<TValue> : IFluentSetterBuilder, IDisposable
{
    private readonly IFluentSetterViewModel _viewModel;
    private bool _disposed;

    protected FluentSetterBuilderBase(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel)
    {
        _viewModel = fluentSetterViewModel ?? throw new ArgumentNullException(nameof(fluentSetterViewModel));
        _valueToSet = valueToSet;
    }

    protected TValue? _valueToSet;

    public bool IsBuilt { get; private set; }

    protected abstract IFluentSetter<TValue> FluentSetter { get; }

    public string GetPropertyName() => FluentSetter.PropertyName;

    internal void ValueToSet(TValue? value) => _valueToSet = value;

    void IFluentSetterBuilder.Build() => Build();

    internal void Build()
    {
        if (IsBuilt)
        {
            throw new InvalidOperationException("Fluent setter is already built.");
        }

        _viewModel.AddFluentSetterBuilder(this);
        IsBuilt = true;
    }

    public virtual void Set()
    {
        if (!IsBuilt)
        {
            Build();
        }

        FluentSetter.Set();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _valueToSet = default;
            if (FluentSetter is IDisposable disposable)
            {
                disposable.Dispose();
            }

            DisposeInternal();
        }

        _disposed = true;
    }

    protected virtual void DisposeInternal()
    {
    }
}
