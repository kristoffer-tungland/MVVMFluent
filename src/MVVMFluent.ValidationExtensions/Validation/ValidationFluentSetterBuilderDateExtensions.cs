using System;

namespace MVVMFluent;

public static partial class ValidationFluentSetterBuilderExtensions
{
    /// <summary>
    /// Ensures a nullable date occurs strictly before today.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="errorMessage">An optional error message to display when the date is not in the past.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static ValidationFluentSetterBuilder<DateTime?> IsDateInPast(
        this ValidationFluentSetterBuilder<DateTime?> builder,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.Date < DateTime.Today,
            ResolveErrorMessage(errorMessage, "Value must be a past date."));
    }

    /// <summary>
    /// Ensures a nullable date occurs strictly after today.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="errorMessage">An optional error message to display when the date is not in the future.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static ValidationFluentSetterBuilder<DateTime?> IsDateInFuture(
        this ValidationFluentSetterBuilder<DateTime?> builder,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.Date > DateTime.Today,
            ResolveErrorMessage(errorMessage, "Value must be a future date."));
    }

    /// <summary>
    /// Ensures a nullable date falls within the specified inclusive range.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimum">The inclusive minimum date.</param>
    /// <param name="maximum">The inclusive maximum date.</param>
    /// <param name="errorMessage">An optional error message to display when the date is outside the range.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static ValidationFluentSetterBuilder<DateTime?> IsDateBetween(
        this ValidationFluentSetterBuilder<DateTime?> builder,
        DateTime minimum,
        DateTime maximum,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        if (minimum > maximum)
        {
            throw new ArgumentException("The minimum date cannot be later than the maximum date.", nameof(minimum));
        }

        var minDate = minimum.Date;
        var maxDate = maximum.Date;

        return builder.Validate(
            value => !value.HasValue || (value.Value.Date >= minDate && value.Value.Date <= maxDate),
            ResolveErrorMessage(errorMessage, $"Value must be between {minDate:d} and {maxDate:d}."));
    }

    /// <summary>
    /// Ensures a nullable date occurs on or after the specified minimum.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimum">The inclusive minimum date.</param>
    /// <param name="errorMessage">An optional error message to display when the date is too early.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static ValidationFluentSetterBuilder<DateTime?> IsOnOrAfter(
        this ValidationFluentSetterBuilder<DateTime?> builder,
        DateTime minimum,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        var minDate = minimum.Date;
        return builder.Validate(
            value => !value.HasValue || value.Value.Date >= minDate,
            ResolveErrorMessage(errorMessage, $"Value must be on or after {minDate:d}."));
    }

    /// <summary>
    /// Ensures a nullable date occurs on or before the specified maximum.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="maximum">The inclusive maximum date.</param>
    /// <param name="errorMessage">An optional error message to display when the date is too late.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static ValidationFluentSetterBuilder<DateTime?> IsOnOrBefore(
        this ValidationFluentSetterBuilder<DateTime?> builder,
        DateTime maximum,
        string? errorMessage = null)
    {
        EnsureBuilder(builder, nameof(builder));

        var maxDate = maximum.Date;
        return builder.Validate(
            value => !value.HasValue || value.Value.Date <= maxDate,
            ResolveErrorMessage(errorMessage, $"Value must be on or before {maxDate:d}."));
    }
}
