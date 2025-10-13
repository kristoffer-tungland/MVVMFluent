using MVVMFluent.Builders;
using MVVMFluent.Interfaces;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MVVMFluent.Validation;

/// <summary>
/// Provides a fluent setter builder that also composes validation rules for the target property.
/// </summary>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public class ValidationFluentSetterBuilder<TValue> : FluentSetterBuilder<TValue>, IValidationFluentSetterBuilder, IValidationFluentSetter<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFluentSetterBuilder{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="fluentSetterViewModel">The owning view model that supports validation.</param>
    /// <param name="propertyName">The name of the property to update. Automatically provided by the compiler when omitted.</param>
    public ValidationFluentSetterBuilder(TValue value, IValidationFluentSetterViewModel fluentSetterViewModel, [CallerMemberName] string? propertyName = null)
        : base(value, fluentSetterViewModel, propertyName)
    {
    }

    /// <inheritdoc />
    protected override FluentSetter<TValue> CreateFluentSetter(IFluentSetterViewModel fluentSetterViewModel, string? propertyName)
    {
        return new ValidationFluentSetter<TValue>((IValidationFluentSetterViewModel)fluentSetterViewModel, propertyName);
    }

    private ValidationFluentSetter<TValue> ValidationSetter => (ValidationFluentSetter<TValue>)FluentSetterInstance;

    /// <inheritdoc />
    public bool HasErrors => ValidationSetter.HasErrors;

    /// <inheritdoc />
    public IEnumerable GetErrors() => ValidationSetter.GetErrors();

    /// <inheritdoc />
    public IValidationFluentSetter<TValue> Validate(params IValidationRule[] rules)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(rules);
        }

        return this;
    }

    /// <inheritdoc />
    public IValidationFluentSetter<TValue> Validate(Func<TValue?, bool> validationFunction, string? errorMessage)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(validationFunction, errorMessage);
        }

        return this;
    }

    /// <inheritdoc />
    public IValidationFluentSetter<TValue> HasValue(string? errorMessage = null)
    {
        return Validate(new RequiredValidationRule(errorMessage));
    }

    /// <inheritdoc />
    [Obsolete("Use HasValue instead.")]
    public IValidationFluentSetter<TValue> Required(string? errorMessage = null)
    {
        return HasValue(errorMessage);
    }

    /// <inheritdoc />
    public void CheckForErrors(object? value)
    {
        ValidationSetter.CheckForErrors(value);
    }
}
