using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent.Interfaces;

/// <summary>
/// Defines a reusable validation rule that can be applied to a value.
/// </summary>
public interface IValidationRule
{
    /// <summary>
    /// Validates the specified value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="cultureInfo">The culture that should be used while validating.</param>
    /// <returns>A <see cref="ValidationResult"/> describing the validation outcome, or <see langword="null"/> when the value is valid.</returns>
    ValidationResult? Validate(object? value, CultureInfo cultureInfo);
}
