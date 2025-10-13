using MVVMFluent.Interfaces;
using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MVVMFluent;

public static partial class ValidationFluentSetterBuilderExtensions
{
    /// <summary>
    /// Adds an email format validation rule for the pending value.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="errorMessage">An optional error message to surface when the value is not a valid email.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> IsEmail(
        this IValidationFluentSetter<string?> builder,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => string.IsNullOrWhiteSpace(value) || IsValidEmail(value!),
            ResolveErrorMessage(errorMessage, "Value must be a valid email address."));
    }

    /// <summary>
    /// Adds an absolute HTTP/HTTPS URL validation rule for the pending value.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="errorMessage">An optional error message to surface when the value is not a URL.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> IsUrl(
        this IValidationFluentSetter<string?> builder,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => string.IsNullOrWhiteSpace(value) || IsValidUrl(value!),
            ResolveErrorMessage(errorMessage, "Value must be a valid URL."));
    }

    /// <summary>
    /// Ensures the value meets a minimum string length.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumLength">The minimum number of characters required.</param>
    /// <param name="errorMessage">An optional error message to display when the value is too short.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> HasMinLength(
        this IValidationFluentSetter<string?> builder,
        int minimumLength,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        if (minimumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "Minimum length cannot be negative.");
        }

        return builder.Validate(
            value => string.IsNullOrEmpty(value) || value!.Length >= minimumLength,
            ResolveErrorMessage(errorMessage, $"Value must be at least {minimumLength} characters long."));
    }

    /// <summary>
    /// Ensures the value does not exceed a maximum string length.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="maximumLength">The maximum number of allowed characters.</param>
    /// <param name="errorMessage">An optional error message to display when the value is too long.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> HasMaxLength(
        this IValidationFluentSetter<string?> builder,
        int maximumLength,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        if (maximumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "Maximum length cannot be negative.");
        }

        return builder.Validate(
            value => string.IsNullOrEmpty(value) || value!.Length <= maximumLength,
            ResolveErrorMessage(errorMessage, $"Value must be {maximumLength} characters or fewer."));
    }

    /// <summary>
    /// Ensures the value falls within a specific string length range.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumLength">The inclusive minimum length.</param>
    /// <param name="maximumLength">The inclusive maximum length.</param>
    /// <param name="errorMessage">An optional error message to display when the value falls outside the range.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> HasLengthBetween(
        this IValidationFluentSetter<string?> builder,
        int minimumLength,
        int maximumLength,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        if (minimumLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumLength), minimumLength, "Minimum length cannot be negative.");
        }

        if (maximumLength < minimumLength)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumLength), maximumLength, "Maximum length cannot be less than the minimum length.");
        }

        return builder.Validate(
            value => string.IsNullOrEmpty(value) || (value!.Length >= minimumLength && value!.Length <= maximumLength),
            ResolveErrorMessage(errorMessage, $"Value length must be between {minimumLength} and {maximumLength} characters."));
    }

    /// <summary>
    /// Validates the value against a regular expression pattern.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="pattern">The pattern the value must match.</param>
    /// <param name="options">Regex options applied to the pattern.</param>
    /// <param name="errorMessage">An optional error message to display when the value fails to match.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<string?> MatchesPattern(
        this IValidationFluentSetter<string?> builder,
        string pattern,
        RegexOptions options = RegexOptions.None,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        if (string.IsNullOrEmpty(pattern))
        {
            throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));
        }

        return builder.Validate(
            value => string.IsNullOrEmpty(value) || Regex.IsMatch(value!, pattern, options),
            ResolveErrorMessage(errorMessage, "Value must match the required format."));
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            var address = new MailAddress(value);
            return string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsValidUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
