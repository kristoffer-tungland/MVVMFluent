using System;
using System.Collections;

namespace MVVMFluent.Interfaces;

/// <summary>
/// Represents a fluent setter builder with validation capabilities.
/// Extends <see cref="IFluentSetter{TValue}"/> with methods for composing validation rules.
/// </summary>
/// <typeparam name="TValue">The type of the property value being set.</typeparam>
public interface IValidationFluentSetter<TValue> : IFluentSetter<TValue>
{
    /// <summary>
    /// Gets the validation errors for this property.
    /// </summary>
    /// <returns>An enumerable collection of error messages.</returns>
    IEnumerable GetErrors();

    /// <summary>
    /// Gets a value indicating whether this property has validation errors.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Adds validation rules to be applied when the property value is set.
    /// </summary>
    /// <param name="rules">The validation rules to apply.</param>
    /// <returns>The current validation fluent setter for method chaining.</returns>
    IValidationFluentSetter<TValue> Validate(params IValidationRule[] rules);

    /// <summary>
    /// Adds a custom validation function that returns true if the value is valid.
    /// </summary>
    /// <param name="validationFunction">A function that returns true if the value passes validation.</param>
    /// <param name="errorMessage">The error message to display if validation fails.</param>
    /// <returns>The current validation fluent setter for method chaining.</returns>
    IValidationFluentSetter<TValue> Validate(Func<TValue?, bool> validationFunction, string? errorMessage);

    /// <summary>
    /// Adds a validation rule that ensures the property has a non-null, non-empty value.
    /// </summary>
    /// <param name="errorMessage">Optional custom error message. If not provided, a default message is used.</param>
    /// <returns>The current validation fluent setter for method chaining.</returns>
    IValidationFluentSetter<TValue> HasValue(string? errorMessage = null);
}
