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

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSetterBuilderBase{TValue}"/> class.
    /// </summary>
    /// <param name="valueToSet">The value that should be assigned when the setter executes.</param>
    /// <param name="fluentSetterViewModel">The owning view model that tracks fluent setters.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fluentSetterViewModel"/> is <see langword="null"/>.</exception>
    protected FluentSetterBuilderBase(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel)
    {
        _viewModel = fluentSetterViewModel ?? throw new ArgumentNullException(nameof(fluentSetterViewModel));
        _valueToSet = valueToSet;
    }

    /// <summary>
    /// Stores the value that should be applied when the fluent setter runs.
    /// </summary>
    protected TValue? _valueToSet;

    /// <summary>
    /// Gets a value indicating whether the builder has been built and registered with the owning view model.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Gets the fluent setter that performs the actual value assignment.
    /// </summary>
    protected abstract IFluentSetter<TValue> FluentSetter { get; }

    /// <summary>
    /// Gets the name of the property targeted by this builder.
    /// </summary>
    /// <returns>The property name associated with the underlying fluent setter.</returns>
    public string GetPropertyName() => FluentSetter.PropertyName;

    internal void ValueToSet(TValue? value) => _valueToSet = value;

    void IFluentSetterBuilder.Build() => Build();

    /// <summary>
    /// Builds the fluent setter and registers it with the owning view model.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the builder has already been built.</exception>
    internal void Build()
    {
        if (IsBuilt)
        {
            throw new InvalidOperationException("Fluent setter is already built.");
        }

        _viewModel.AddFluentSetterBuilder(this);
        IsBuilt = true;
    }

    /// <summary>
    /// Applies the configured value to the underlying property.
    /// </summary>
    public virtual void Set()
    {
        if (!IsBuilt)
        {
            Build();
        }

        FluentSetter.Set();
    }

    /// <summary>
    /// Releases the resources used by the builder.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the builder and optionally disposes of managed resources.
    /// </summary>
    /// <param name="disposing">A value indicating whether to dispose managed resources.</param>
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

    /// <summary>
    /// Performs additional cleanup logic when the builder is disposed.
    /// </summary>
    protected virtual void DisposeInternal()
    {
    }
}
