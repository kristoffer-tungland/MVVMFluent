namespace MVVMFluent.Interfaces;

/// <summary>
/// Represents a builder that creates and configures a fluent setter instance.
/// </summary>
public interface IFluentSetterBuilder
{
    /// <summary>
    /// Gets a value indicating whether the builder has been executed and the setter registered.
    /// </summary>
    bool IsBuilt { get; }

    /// <summary>
    /// Completes the builder configuration and registers it with the owning view model.
    /// </summary>
    void Build();

    /// <summary>
    /// Gets the name of the property associated with the fluent setter produced by this builder.
    /// </summary>
    /// <returns>The property name targeted by the builder.</returns>
    string GetPropertyName();
}
