namespace MVVMFluent
{
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
        /// <returns>A <see cref="global::System.ComponentModel.DataAnnotations.ValidationResult"/> describing the validation outcome, or <see langword="null"/> when the value is valid.</returns>
        global::System.ComponentModel.DataAnnotations.ValidationResult? Validate(object? value, global::System.Globalization.CultureInfo cultureInfo);
    }
}
