using System.Collections;

namespace MVVMFluent.Interfaces;

/// <summary>
/// Represents a fluent setter builder that supports validation of property values.
/// </summary>
public interface IValidationFluentSetterBuilder : IFluentSetterBuilder
{
    /// <summary>
    /// Gets a value indicating whether the builder currently has validation errors.
    /// </summary>
    bool HasErrors { get; }

    /// <summary>
    /// Evaluates the configured validation rules against the provided value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    void CheckForErrors(object? value);

    /// <summary>
    /// Gets the collection of validation errors produced by the builder.
    /// </summary>
    /// <returns>An enumerable collection of error messages.</returns>
    IEnumerable GetErrors();
}