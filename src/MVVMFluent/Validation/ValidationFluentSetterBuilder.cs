using MVVMFluent.Builders;
using MVVMFluent.Interfaces;
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MVVMFluent.Validation;

public class ValidationFluentSetterBuilder<TValue> : FluentSetterBuilder<TValue>, IValidationFluentSetterBuilder, IValidationFluentSetter<TValue>
{
    public ValidationFluentSetterBuilder(TValue value, IValidationFluentSetterViewModel fluentSetterViewModel, [CallerMemberName] string? propertyName = null)
        : base(value, fluentSetterViewModel, propertyName)
    {
    }

    protected override FluentSetter<TValue> CreateFluentSetter(IFluentSetterViewModel fluentSetterViewModel, string? propertyName)
    {
        return new ValidationFluentSetter<TValue>((IValidationFluentSetterViewModel)fluentSetterViewModel, propertyName);
    }

    private ValidationFluentSetter<TValue> ValidationSetter => (ValidationFluentSetter<TValue>)FluentSetterInstance;

    public bool HasErrors => ValidationSetter.HasErrors;

    public IEnumerable GetErrors() => ValidationSetter.GetErrors();

    public IValidationFluentSetter<TValue> Validate(params IValidationRule[] rules)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(rules);
        }

        return this;
    }

    public IValidationFluentSetter<TValue> Validate(Func<TValue?, bool> validationFunction, string? errorMessage)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(validationFunction, errorMessage);
        }

        return this;
    }

    public IValidationFluentSetter<TValue> HasValue(string? errorMessage = null)
    {
        return Validate(new RequiredValidationRule(errorMessage));
    }

    [Obsolete("Use HasValue instead.")]
    public IValidationFluentSetter<TValue> Required(string? errorMessage = null)
    {
        return HasValue(errorMessage);
    }

    public void CheckForErrors(object? value)
    {
        ValidationSetter.CheckForErrors(value);
    }
}
