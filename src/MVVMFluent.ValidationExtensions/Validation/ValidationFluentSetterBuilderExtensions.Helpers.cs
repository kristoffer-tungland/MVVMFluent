using System;

namespace MVVMFluent;

/// <summary>
/// Provides reusable validation helpers for <see cref="ValidationFluentSetterBuilder{TValue}"/>.
/// </summary>
public static partial class ValidationFluentSetterBuilderExtensions
{
    private static string ResolveErrorMessage(string? errorMessage, string defaultMessage)
    {
        return string.IsNullOrWhiteSpace(errorMessage) ? defaultMessage : errorMessage!;
    }

    private static void EnsureBuilder<TBuilder>(TBuilder? builder, string parameterName)
        where TBuilder : class
    {
        if (builder is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
}
