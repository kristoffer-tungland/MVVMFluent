using MVVMFluent.Builders;
using MVVMFluent.Interfaces;
using System;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that provides property change notification and command creation.
/// </summary>
public abstract class ViewModelBase : FluentSetterViewModelBase
{
    /// <summary>
    /// Creates a fluent setter builder for configuring property change behavior.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="value">The new value to set for the property.</param>
    /// <param name="propertyName">The name of the property. Automatically captured from the caller member name.</param>
    /// <returns>An <see cref="IFluentSetter{TValue}"/> that allows configuring change callbacks and notifications before committing the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the property name cannot be determined.</exception>
    /// <example>
    /// <code>
    /// public string Name
    /// {
    ///     get => Get&lt;string&gt;();
    ///     set => When(value)
    ///         .Changed(() => Console.WriteLine("Name changed"))
    ///         .Notify(SaveCommand)
    ///         .Set();
    /// }
    /// </code>
    /// </example>
    protected IFluentSetter<TValue> When<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
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
