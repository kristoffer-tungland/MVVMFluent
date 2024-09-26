namespace MVVMFluent.WPF.ValidationRules
{
    public class RequiredValidationRule : global::System.Windows.Controls.ValidationRule
    {
        private readonly string? _errorMessage;

        public RequiredValidationRule(string? errorMessage = default)
        {
            _errorMessage = errorMessage;
        }

        public override global::System.Windows.Controls.ValidationResult Validate(object? value, 
            global::System.Globalization.CultureInfo cultureInfo)
        {
            if (value is null)
                return new global::System.Windows.Controls.ValidationResult(false, _errorMessage ?? "Value is required");

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return new global::System.Windows.Controls.ValidationResult(false, _errorMessage ?? "Value is required");

            return global::System.Windows.Controls.ValidationResult.ValidResult;
        }
    }
}