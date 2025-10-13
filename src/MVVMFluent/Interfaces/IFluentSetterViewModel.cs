namespace MVVMFluent.Interfaces;

/// <summary>
/// Defines the contract required by view models that support fluent property setters.
/// </summary>
public interface IFluentSetterViewModel
{
    /// <summary>
    /// Registers a fluent setter builder with the view model.
    /// </summary>
    /// <param name="fluentSetterBuilder">The builder to register.</param>
    void AddFluentSetterBuilder(IFluentSetterBuilder fluentSetterBuilder);

    /// <summary>
    /// Retrieves a fluent setter builder for the specified property if one exists.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The fluent setter builder for the property, or <see langword="null"/> if none is registered.</returns>
    IFluentSetterBuilder? GetFluentSetterBuilder(string propertyName);

    /// <summary>
    /// Gets the current backing-field value for the specified property.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>The stored value for the property.</returns>
    T? GetFieldValue<T>(string propertyName);

    /// <summary>
    /// Sets the backing-field value for the specified property.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to store.</param>
    void SetFieldValue<T>(string propertyName, T? value);

    /// <summary>
    /// Raises a <c>PropertyChanged</c> notification for the specified property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    void OnPropertyChanged(string? propertyName);
}
