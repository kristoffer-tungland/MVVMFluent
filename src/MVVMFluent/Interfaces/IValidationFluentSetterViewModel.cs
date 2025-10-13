using System.ComponentModel;

namespace MVVMFluent.Interfaces;

/// <summary>
/// Represents a view model that supports fluent setters with validation capabilities.
/// </summary>
public interface IValidationFluentSetterViewModel : IFluentSetterViewModel, INotifyDataErrorInfo
{
    /// <summary>
    /// Triggers validation for the specified property using its current value.
    /// </summary>
    /// <param name="propertyName">The name of the property to validate.</param>
    void CheckErrorsFor(string? propertyName);

    /// <summary>
    /// Raises a <see cref="INotifyDataErrorInfo.ErrorsChanged"/> event for the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property whose errors changed.</param>
    void RaiseErrorsChanged(string? propertyName);
}
