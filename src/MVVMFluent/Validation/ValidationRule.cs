namespace MVVMFluent
{
    /// <summary>
    /// Represents a base class for creating validation rules.
    /// </summary>
    public abstract class ValidationRule : IValidationRule
    {
        /// <inheritdoc />
        public abstract global::System.ComponentModel.DataAnnotations.ValidationResult? Validate(object? value, global::System.Globalization.CultureInfo cultureInfo);
    }
}
