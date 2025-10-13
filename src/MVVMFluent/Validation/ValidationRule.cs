using MVVMFluent.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent.Validation;

/// <summary>
/// Represents a base class for creating validation rules.
/// </summary>
public abstract class ValidationRule : IValidationRule
{
    /// <summary>
    /// Validates the specified value using the provided culture.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="cultureInfo">The culture to use when performing validation.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the validation outcome, or <see langword="null"/> when the value is valid.</returns>
    public abstract ValidationResult? Validate(object? value, CultureInfo cultureInfo);
}
