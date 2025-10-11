namespace MVVMFluent
{
    public class RequiredValidationRule : ValidationRule
    {
        private readonly string? _errorMessage;

        public RequiredValidationRule(string? errorMessage = default)
        {
            _errorMessage = errorMessage;
        }

        public override global::System.ComponentModel.DataAnnotations.ValidationResult? Validate(object? value, global::System.Globalization.CultureInfo cultureInfo)
        {
            if (value is null)
                return new global::System.ComponentModel.DataAnnotations.ValidationResult(_errorMessage ?? "Value is required");

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return new global::System.ComponentModel.DataAnnotations.ValidationResult(_errorMessage ?? "Value is required");

            return global::System.ComponentModel.DataAnnotations.ValidationResult.Success;
        }
    }
}
