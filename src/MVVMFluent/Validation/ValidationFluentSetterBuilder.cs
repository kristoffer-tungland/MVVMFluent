using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

public class ValidationFluentSetterBuilder<TValue> : FluentSetterBuilder<TValue>, IValidationFluentSetterBuilder
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

    public ValidationFluentSetterBuilder<TValue> Validate(params IValidationRule[] rules)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(rules);
        }

        return this;
    }

    public ValidationFluentSetterBuilder<TValue> Validate(Func<TValue?, bool> validationFunction, string? errorMessage)
    {
        if (!IsBuilt)
        {
            ValidationSetter.Validate(validationFunction, errorMessage);
        }

        return this;
    }

    public ValidationFluentSetterBuilder<TValue> HasValue(string? errorMessage = null)
    {
        return Validate(new RequiredValidationRule(errorMessage));
    }

    public void CheckForErrors(object? value)
    {
        ValidationSetter.CheckForErrors(value);
    }
}
