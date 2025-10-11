using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for creating validation rules.
/// </summary>
public abstract class ValidationRule : IValidationRule
{
    public abstract ValidationResult? Validate(object? value, CultureInfo cultureInfo);
}
