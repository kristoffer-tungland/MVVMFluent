using System;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that provides property change notification and command creation.
/// </summary>
public abstract class ViewModelBase : FluentSetterViewModelBase
{
    protected FluentSetterBuilder<TValue> When<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");
        }

        if (GetFluentSetterBuilder(propertyName) is FluentSetterBuilder<TValue> existingBuilder)
        {
            existingBuilder.ValueToSet(value);
            return existingBuilder;
        }

        return new FluentSetterBuilder<TValue>(value, this, propertyName);
    }
}
