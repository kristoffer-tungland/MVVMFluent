using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent;

public class RequiredValidationRule : ValidationRule
{
    private readonly string? _errorMessage;

    public RequiredValidationRule(string? errorMessage = null)
    {
        _errorMessage = errorMessage;
    }

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
