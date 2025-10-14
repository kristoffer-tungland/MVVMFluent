using MVVMFluent.Commands;
using MVVMFluent.Interfaces;
using MVVMFluent.Queries;
using MVVMFluent.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private readonly Dictionary<string, object> _queryBuilderStore = new();
    private IQueryDispatcher? _queryDispatcher;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationViewModelBase"/> class with the specified query dispatcher.
    /// </summary>
    /// <param name="queryDispatcher">The dispatcher responsible for executing queries.</param>
    protected ValidationViewModelBase(IQueryDispatcher queryDispatcher)
        : this()
    {
        UseQueries(queryDispatcher);
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
    /// <example>
    /// <code>
    /// public string Email
    /// {
    ///     get =&gt; Get&lt;string&gt;();
    ///     set =&gt; When(value)
    ///         .HasValue("Email is required")
    ///         .Validate(v =&gt; v?.Contains('@') == true, "Email must contain '@'")
    ///         .Notify(SaveCommand)
    ///         .Set();
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="value">The new value to set for the property.</param>
    /// <param name="propertyName">The name of the property. Automatically captured from the caller member name.</param>
    /// <returns>An <see cref="IValidationFluentSetter{TValue}"/> that allows configuring validation rules, change callbacks, and notifications before committing the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the property name cannot be determined.</exception>

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

    /// <summary>
    /// Configures the view model to use the supplied <see cref="IQueryDispatcher"/>.
    /// </summary>
    /// <param name="queryDispatcher">The dispatcher responsible for executing queries.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="queryDispatcher"/> is <see langword="null"/>.</exception>
    protected void UseQueries(IQueryDispatcher queryDispatcher)
    {
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
    }

    /// <summary>
    /// Creates a query command builder that dispatches the supplied query when executed.
    /// </summary>
    /// <typeparam name="TResult">The type of result produced by the query.</typeparam>
    /// <param name="factory">The factory used to create the query instance.</param>
    /// <param name="propertyName">The property name used for caching. Automatically provided by the compiler.</param>
    /// <returns>A <see cref="QueryAsyncCommandBuilder{TResult}"/> for configuring the command.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="propertyName"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no query dispatcher has been configured.</exception>
    protected QueryAsyncCommandBuilder<TResult> Send<TResult>(Func<IQuery<TResult>> factory, [CallerMemberName] string? propertyName = null)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        EnsurePropertyName(propertyName);

        if (_queryDispatcher == null)
        {
            throw new InvalidOperationException("No IQueryDispatcher has been configured. Provide one via the constructor or call UseQueries before creating query commands.");
        }

        if (_queryBuilderStore.TryGetValue(propertyName!, out var existingBuilder))
        {
            return (QueryAsyncCommandBuilder<TResult>)existingBuilder;
        }

        var builder = new QueryAsyncCommandBuilder<TResult>(this, factory, _queryDispatcher);
        _queryBuilderStore[propertyName!] = builder;
        _commandStore[propertyName!] = builder.Command;
        return builder;
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
        _queryBuilderStore.Clear();
        ErrorsChanged -= ErrorsChangedHandler;
        ErrorsChanged = null;

        base.DisposeInternal();
    }

    private static void EnsurePropertyName(string? propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException(nameof(propertyName), "Not able to determine property name.");
        }

        if (propertyName == ".ctor")
        {
            throw new ArgumentException("Property name must be provided when it is used inside a constructor.", nameof(propertyName));
        }
    }
}
