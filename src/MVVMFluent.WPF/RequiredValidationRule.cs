namespace MVVMFluent.WPF
{
    public class RequiredValidationRule : System.Windows.Controls.ValidationRule
    {
        private readonly string? _errorMessage;

        public RequiredValidationRule(string? errorMessage = default)
        {
            _errorMessage = errorMessage;
        }

        public override System.Windows.Controls.ValidationResult Validate(object? value,
            System.Globalization.CultureInfo cultureInfo)
        {
            if (value is null)
                return new System.Windows.Controls.ValidationResult(false, _errorMessage ?? "Value is required");

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return new System.Windows.Controls.ValidationResult(false, _errorMessage ?? "Value is required");

            return System.Windows.Controls.ValidationResult.ValidResult;
        }
    }
}