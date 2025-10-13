using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent.Validation;

/// <summary>
/// Represents a validation rule that ensures a value is not null or whitespace.
/// </summary>
public class RequiredValidationRule : ValidationRule
{
    private readonly string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredValidationRule"/> class.
    /// </summary>
    /// <param name="errorMessage">An optional custom error message to use when validation fails.</param>
    public RequiredValidationRule(string? errorMessage = null)
    {
        _errorMessage = errorMessage;
    }

    /// <inheritdoc />
    public override ValidationResult? Validate(object? value, CultureInfo cultureInfo)
    {
        if (value == null)
        {
            return new ValidationResult(_errorMessage ?? "Value is required");
        }

        if (value is string str && string.IsNullOrWhiteSpace(str))
        {
            return new ValidationResult(_errorMessage ?? "Value is required");
        }

        return ValidationResult.Success;
    }
}
