using System;
using System.Windows.Input;

namespace MVVMFluent.Interfaces;

/// <summary>
/// Represents a fluent setter builder that provides a chainable API for configuring property change behavior.
/// </summary>
/// <typeparam name="TValue">The type of the property value being set.</typeparam>
public interface IFluentSetter<TValue>
{
    /// <summary>
    /// Gets the name of the property associated with this fluent setter.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Commits the configured value to the property, triggering any registered callbacks and notifications.
    /// </summary>
    void Set();

    /// <summary>
    /// Registers an action to be invoked before the property value changes.
    /// </summary>
    /// <param name="action">The action to invoke before the value changes.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changing(Action action);

    /// <summary>
    /// Registers an action to be invoked before the property value changes, providing the new value.
    /// </summary>
    /// <param name="action">The action to invoke with the new value before the change occurs.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changing(Action<TValue?> action);

    /// <summary>
    /// Registers an action to be invoked before the property value changes, providing both old and new values.
    /// </summary>
    /// <param name="action">The action to invoke with the old and new values before the change occurs.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changing(Action<TValue?, TValue?> action);

    /// <summary>
    /// Registers an action to be invoked after the property value has changed.
    /// </summary>
    /// <param name="action">The action to invoke after the value changes.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changed(Action action);

    /// <summary>
    /// Registers an action to be invoked after the property value has changed, providing the new value.
    /// </summary>
    /// <param name="action">The action to invoke with the new value after the change occurs.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changed(Action<TValue?> action);

    /// <summary>
    /// Registers an action to be invoked after the property value has changed, providing both old and new values.
    /// </summary>
    /// <param name="action">The action to invoke with the old and new values after the change occurs.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Changed(Action<TValue?, TValue?> action);

    /// <summary>
    /// Registers commands whose CanExecute state should be re-evaluated when this property changes.
    /// </summary>
    /// <param name="commands">The commands to notify of potential CanExecute changes.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Notify(params ICommand[] commands);

    /// <summary>
    /// Registers additional property names that should raise PropertyChanged notifications when this property changes.
    /// </summary>
    /// <param name="propertyNames">The names of dependent properties that should be notified.</param>
    /// <returns>The current fluent setter for method chaining.</returns>
    IFluentSetter<TValue> Notify(params string[] propertyNames);
}
