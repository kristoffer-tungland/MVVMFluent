using MVVMFluent.Interfaces;
using System;

namespace MVVMFluent;

public static partial class ValidationFluentSetterBuilderExtensions
{
    /// <summary>
    /// Adds an age range validation rule for non-nullable integer values.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumAge">The inclusive minimum age.</param>
    /// <param name="maximumAge">The inclusive maximum age.</param>
    /// <param name="errorMessage">An optional error message to display when the value is out of range.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<int> IsAgeBetween(
        this IValidationFluentSetter<int> builder,
        int minimumAge,
        int maximumAge,
        string? errorMessage = null)
    {
        return builder.ValidateAgeRange(minimumAge, maximumAge, errorMessage);
    }

    /// <summary>
    /// Adds an age range validation rule for nullable integer values.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumAge">The inclusive minimum age.</param>
    /// <param name="maximumAge">The inclusive maximum age.</param>
    /// <param name="errorMessage">An optional error message to display when the value is out of range.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<int?> IsAgeBetween(
        this IValidationFluentSetter<int?> builder,
        int minimumAge,
        int maximumAge,
        string? errorMessage = null)
    {
        return builder.ValidateAgeRange(minimumAge, maximumAge, errorMessage);
    }

    /// <summary>
    /// Ensures a non-nullable age meets the required minimum.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumAge">The inclusive minimum age.</param>
    /// <param name="errorMessage">An optional error message to display when the value is below the minimum.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<int> IsMinimumAge(
        this IValidationFluentSetter<int> builder,
        int minimumAge,
        string? errorMessage = null)
    {
        return builder.ValidateMinimumAge(minimumAge, errorMessage);
    }

    /// <summary>
    /// Ensures a nullable age meets the required minimum when provided.
    /// </summary>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimumAge">The inclusive minimum age.</param>
    /// <param name="errorMessage">An optional error message to display when the value is below the minimum.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<int?> IsMinimumAge(
        this IValidationFluentSetter<int?> builder,
        int minimumAge,
        string? errorMessage = null)
    {
        return builder.ValidateMinimumAge(minimumAge, errorMessage);
    }

    /// <summary>
    /// Validates that a nullable value falls within the specified range.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="builder">The validation builder.</param>
    /// <param name="minimum">The inclusive minimum value.</param>
    /// <param name="maximum">The inclusive maximum value.</param>
    /// <param name="errorMessage">An optional error message to display when the value is out of range.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<T?> IsInRange<T>(
        this IValidationFluentSetter<T?> builder,
        T minimum,
        T maximum,
        string? errorMessage = null)
        where T : struct, IComparable<T>
    {
        EnsureBuilder(builder, nameof(builder));

        if (minimum.CompareTo(maximum) > 0)
        {
            throw new ArgumentException("The minimum value cannot be greater than the maximum value.", nameof(minimum));
        }

        return builder.Validate(
            value => !value.HasValue || (value.Value.CompareTo(minimum) >= 0 && value.Value.CompareTo(maximum) <= 0),
            ResolveErrorMessage(errorMessage, $"Value must be between {minimum} and {maximum}."));
    }

    /// <summary>
    /// Ensures a nullable value is strictly greater than the supplied threshold when present.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="builder">The validation builder.</param>
    /// <param name="threshold">The exclusive lower bound.</param>
    /// <param name="errorMessage">An optional error message to display when the value is not greater.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<T?> IsGreaterThan<T>(
        this IValidationFluentSetter<T?> builder,
        T threshold,
        string? errorMessage = null)
        where T : struct, IComparable<T>
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.CompareTo(threshold) > 0,
            ResolveErrorMessage(errorMessage, $"Value must be greater than {threshold}."));
    }

    /// <summary>
    /// Ensures a nullable value is greater than or equal to the supplied threshold when present.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="builder">The validation builder.</param>
    /// <param name="threshold">The inclusive lower bound.</param>
    /// <param name="errorMessage">An optional error message to display when the value is below the threshold.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<T?> IsGreaterThanOrEqualTo<T>(
        this IValidationFluentSetter<T?> builder,
        T threshold,
        string? errorMessage = null)
        where T : struct, IComparable<T>
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.CompareTo(threshold) >= 0,
            ResolveErrorMessage(errorMessage, $"Value must be greater than or equal to {threshold}."));
    }

    /// <summary>
    /// Ensures a nullable value is strictly less than the supplied threshold when present.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="builder">The validation builder.</param>
    /// <param name="threshold">The exclusive upper bound.</param>
    /// <param name="errorMessage">An optional error message to display when the value is not less.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<T?> IsLessThan<T>(
        this IValidationFluentSetter<T?> builder,
        T threshold,
        string? errorMessage = null)
        where T : struct, IComparable<T>
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.CompareTo(threshold) < 0,
            ResolveErrorMessage(errorMessage, $"Value must be less than {threshold}."));
    }

    /// <summary>
    /// Ensures a nullable value is less than or equal to the supplied threshold when present.
    /// </summary>
    /// <typeparam name="T">The numeric struct type.</typeparam>
    /// <param name="builder">The validation builder.</param>
    /// <param name="threshold">The inclusive upper bound.</param>
    /// <param name="errorMessage">An optional error message to display when the value exceeds the threshold.</param>
    /// <returns>The supplied builder for fluent chaining.</returns>
    public static IValidationFluentSetter<T?> IsLessThanOrEqualTo<T>(
        this IValidationFluentSetter<T?> builder,
        T threshold,
        string? errorMessage = null)
        where T : struct, IComparable<T>
    {
        EnsureBuilder(builder, nameof(builder));

        return builder.Validate(
            value => !value.HasValue || value.Value.CompareTo(threshold) <= 0,
            ResolveErrorMessage(errorMessage, $"Value must be less than or equal to {threshold}."));
    }

    private static IValidationFluentSetter<int> ValidateAgeRange(
        this IValidationFluentSetter<int> builder,
        int minimumAge,
        int maximumAge,
        string? errorMessage)
    {
        EnsureBuilder(builder, nameof(builder));
        ValidateAgeBounds(minimumAge, maximumAge);

        return builder.Validate(
            validationFunction: value => value >= minimumAge && value <= maximumAge,
            errorMessage: ResolveErrorMessage(errorMessage, $"Value must represent an age between {minimumAge} and {maximumAge}."));
    }

    private static IValidationFluentSetter<int?> ValidateAgeRange(
        this IValidationFluentSetter<int?> builder,
        int minimumAge,
        int maximumAge,
        string? errorMessage)
    {
        EnsureBuilder(builder, nameof(builder));
        ValidateAgeBounds(minimumAge, maximumAge);

        return builder.Validate(
            value => !value.HasValue || (value.Value >= minimumAge && value.Value <= maximumAge),
            ResolveErrorMessage(errorMessage, $"Value must represent an age between {minimumAge} and {maximumAge}."));
    }

    private static IValidationFluentSetter<int> ValidateMinimumAge(
        this IValidationFluentSetter<int> builder,
        int minimumAge,
        string? errorMessage)
    {
        EnsureBuilder(builder, nameof(builder));
        ValidateMinimumAgeValue(minimumAge);

        return builder.Validate(
            validationFunction: value => value >= minimumAge,
            errorMessage: ResolveErrorMessage(errorMessage, $"Value must represent an age of at least {minimumAge}."));
    }

    private static IValidationFluentSetter<int?> ValidateMinimumAge(
        this IValidationFluentSetter<int?> builder,
        int minimumAge,
        string? errorMessage)
    {
        EnsureBuilder(builder, nameof(builder));
        ValidateMinimumAgeValue(minimumAge);

        return builder.Validate(
            value => !value.HasValue || value.Value >= minimumAge,
            ResolveErrorMessage(errorMessage, $"Value must represent an age of at least {minimumAge}."));
    }

    private static void ValidateAgeBounds(int minimumAge, int maximumAge)
    {
        ValidateMinimumAgeValue(minimumAge);

        if (maximumAge < minimumAge)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAge), maximumAge, "Maximum age cannot be less than the minimum age.");
        }
    }

    private static void ValidateMinimumAgeValue(int minimumAge)
    {
        if (minimumAge < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumAge), minimumAge, "Age cannot be negative.");
        }
    }
}
