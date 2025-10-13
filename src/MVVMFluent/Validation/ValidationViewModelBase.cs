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
    public bool HasErrors { get; private set; }

    public ObservableCollection<string> Errors { get; } = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

    public void RaiseErrorsChanged(string? propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

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

    protected ValidationFluentSetterBuilder<TValue> When<TValue>(TValue value, [CallerMemberName] string? propertyName = null)
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

    protected override void DisposeInternal()
    {
        ErrorsChanged -= ErrorsChangedHandler;
        ErrorsChanged = null;

        base.DisposeInternal();
    }
}
