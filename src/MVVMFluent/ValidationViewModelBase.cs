using MVVMFluent.Interfaces;
using MVVMFluent.Validation;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MVVMFluent;

/// <summary>
/// Represents a base class for view models that provides property change notification, command creation, and validation.
/// </summary>
public abstract class ValidationViewModelBase : FluentSetterViewModelBase, IValidationFluentSetterViewModel
{
    /// <summary>
    /// Gets a value indicating whether any properties in this view model have validation errors.
    /// </summary>
    public bool HasErrors { get; private set; }

    /// <summary>
    /// Gets a collection of formatted validation error messages for all properties that have errors.
    /// Each error is formatted as "PropertyName: error message".
    /// </summary>
    public ObservableCollection<string> Errors { get; } = new();

    /// <summary>
    /// Occurs when the validation errors have changed for a property or for the entire entity.
    /// </summary>
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationViewModelBase"/> class.
    /// </summary>
    protected ValidationViewModelBase()
    {
        ErrorsChanged += ErrorsChangedHandler;
    }

    private void ErrorsChangedHandler(object? sender, DataErrorsChangedEventArgs e)
    {
        Errors.Clear();
        var builders = _builderStore.Values.OfType<IValidationFluentSetterBuilder>().ToList();

        foreach (var builder in builders)
        {
            var messages = builder.GetErrors().OfType<string>().ToList();

            if (messages.Count == 0)
            {
                continue;
            }

            var propertyName = builder.GetPropertyName();
            Errors.Add($"{propertyName}: {string.Join(", ", messages)}");
        }

        HasErrors = builders.Any(x => x.HasErrors);
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property whose errors have changed.</param>
    public void RaiseErrorsChanged(string? propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Gets the validation errors for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve errors for, or null to get all errors.</param>
    /// <returns>An enumerable collection of error messages for the specified property.</returns>
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return GetAllErrors();
        }

        return GetFluentSetterBuilder(propertyName!) is IValidationFluentSetterBuilder validationBuilder
            ? validationBuilder.GetErrors()
            : Array.Empty<string>();
    }

    /// <summary>
    /// Manually triggers validation for the specified property using its current value.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when propertyName is null or empty.</exception>
    public void CheckErrorsFor(string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
        }

        if (GetFluentSetterBuilder(propertyName!) is not IValidationFluentSetterBuilder validationBuilder)
        {
            return;
        }

        var property = GetType().GetProperty(propertyName!);
        var value = property?.GetValue(this);

        validationBuilder.CheckForErrors(value);
    }

    /// <summary>
    /// Creates a validation fluent setter builder for configuring property change behavior with validation rules.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="value">The new value to set for the property.</param>
    /// <param name="propertyName">The name of the property. Automatically captured from the caller member name.</param>
    /// <returns>An <see cref="IValidationFluentSetter{TValue}"/> that allows configuring validation rules, change callbacks, and notifications before committing the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the property name cannot be determined.</exception>
    /// <example>
    /// <code>
    /// public string Email
    /// {
    ///     get => Get&lt;string&gt;();
    ///     set => When(value)
    ///         .HasValue("Email is required")
    ///         .Validate(v => v?.Contains('@') == true, "Email must contain '@'")
    ///         .Notify(SaveCommand)
    ///         .Set();
    /// }
    /// </code>
    /// </example>
    protected IValidationFluentSetter<TValue> When<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");
        }

        if (GetFluentSetterBuilder(propertyName) is ValidationFluentSetterBuilder<TValue> existingBuilder)
        {
            existingBuilder.ValueToSet(value);
            return existingBuilder;
        }

        return new ValidationFluentSetterBuilder<TValue>(value, this, propertyName);
    }

    private IEnumerable GetAllErrors()
    {
        return _builderStore.Values
            .OfType<IValidationFluentSetterBuilder>()
            .SelectMany(builder => builder.GetErrors().OfType<string>());
    }

    /// <summary>
    /// Disposes resources used by this view model, including unsubscribing from the ErrorsChanged event.
    /// </summary>
    protected override void DisposeInternal()
    {
        ErrorsChanged -= ErrorsChangedHandler;
        ErrorsChanged = null;

        base.DisposeInternal();
    }
}
