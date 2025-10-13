using MVVMFluent.Builders;
using MVVMFluent.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent.Validation;

public class ValidationFluentSetter<TValue> : FluentSetter<TValue>, IValidationFluentSetter<TValue>
{
    private readonly List<IValidationRule> _rules = new();
    private readonly IValidationFluentSetterViewModel _viewModel;
    private Func<TValue?, bool>? _validationFunction;
    private string? _errorMessage;

    public ValidationFluentSetter(IValidationFluentSetterViewModel viewModel, string? propertyName)
        : base(viewModel, propertyName)
    {
        _viewModel = viewModel;
    }

    public ObservableCollection<string> Errors { get; } = new();

    public bool HasErrors { get; private set; }

    public IEnumerable GetErrors() => Errors;

    public IValidationFluentSetter<TValue> Validate(params IValidationRule[] rules)
    {
        if (rules == null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        foreach (var rule in rules)
        {
            AddRule(rule);
        }

        return this;
    }

    public IValidationFluentSetter<TValue> Validate(Func<TValue?, bool> validationFunction, string? errorMessage)
    {
        if (validationFunction == null)
        {
            throw new ArgumentNullException(nameof(validationFunction));
        }

        _validationFunction = validationFunction;
        _errorMessage = errorMessage;
        return this;
    }

    public IValidationFluentSetter<TValue> HasValue(string? errorMessage = null)
    {
        return Validate(new RequiredValidationRule(errorMessage));
    }

    internal ValidationFluentSetter<TValue> AddRule(IValidationRule rule)
    {
        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        if (!_rules.Contains(rule))
        {
            _rules.Add(rule);
        }

        return this;
    }

    public override void Set()
    {
        CheckForErrors(ValueToSet);
        base.Set();
    }

    public void CheckForErrors(TValue? valueToSet)
    {
        CheckForErrors((object?)valueToSet);
    }

    public void CheckForErrors(object? valueToSet)
    {
        Errors.Clear();
        HasErrors = false;

        foreach (var rule in _rules)
        {
            var validationResult = rule.Validate(valueToSet, CultureInfo.CurrentCulture);

            if (validationResult == null || validationResult == ValidationResult.Success)
            {
                continue;
            }

            var errorMessage = validationResult.ErrorMessage;
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new InvalidOperationException("Validation rule did not return an error message.");
            }

            Errors.Add(errorMessage);
            HasErrors = true;
        }

        if (_validationFunction != null)
        {
            var isValid = _validationFunction((TValue?)valueToSet);
            if (!isValid)
            {
                Errors.Add(_errorMessage ?? "Validation failed");
                HasErrors = true;
            }
        }

        _viewModel.RaiseErrorsChanged(PropertyName);
    }
}
