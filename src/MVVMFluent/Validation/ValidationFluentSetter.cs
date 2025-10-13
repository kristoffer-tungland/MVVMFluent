using MVVMFluent.Builders;
using MVVMFluent.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MVVMFluent.Validation;

/// <summary>
/// Extends <see cref="FluentSetter{TValue}"/> with validation rule composition for property values.
/// </summary>
/// <typeparam name="TValue">The type of the property value.</typeparam>
public class ValidationFluentSetter<TValue> : FluentSetter<TValue>, IValidationFluentSetter<TValue>
{
    private readonly List<IValidationRule> _rules = new();
    private readonly IValidationFluentSetterViewModel _viewModel;
    private Func<TValue?, bool>? _validationFunction;
    private string? _errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFluentSetter{TValue}"/> class.
    /// </summary>
    /// <param name="viewModel">The owning view model that supports validation.</param>
    /// <param name="propertyName">The name of the property to update.</param>
    public ValidationFluentSetter(IValidationFluentSetterViewModel viewModel, string? propertyName)
        : base(viewModel, propertyName)
    {
        _viewModel = viewModel;
    }

    /// <summary>
    /// Gets the collection of validation error messages for the property.
    /// </summary>
    public ObservableCollection<string> Errors { get; } = new();

    /// <summary>
    /// Gets a value indicating whether validation errors are present.
    /// </summary>
    public bool HasErrors { get; private set; }

    /// <inheritdoc />
    public IEnumerable GetErrors() => Errors;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IValidationFluentSetter<TValue> HasValue(string? errorMessage = null)
    {
        return Validate(new RequiredValidationRule(errorMessage));
    }

    /// <summary>
    /// Adds a validation rule to the current builder.
    /// </summary>
    /// <param name="rule">The validation rule to add.</param>
    /// <returns>The current <see cref="ValidationFluentSetter{TValue}"/> instance.</returns>
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

    /// <inheritdoc />
    public override void Set()
    {
        CheckForErrors(ValueToSet);
        base.Set();
    }

    /// <summary>
    /// Executes validation for the specified value.
    /// </summary>
    /// <param name="valueToSet">The value to validate.</param>
    public void CheckForErrors(TValue? valueToSet)
    {
        CheckForErrors((object?)valueToSet);
    }

    /// <inheritdoc />
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
