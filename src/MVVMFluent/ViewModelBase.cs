namespace MVVMFluent
{
    /// <summary>
    /// Represents a base class for view models that provides property change notification and command creation.
    /// <example>
    /// <code lang="csharp">
    /// public class MainViewModel : ViewModelBase
    /// {
    ///     // Property with notification and default value
    ///     public bool Enabled { get => Get(true); set => Set(value); }
    ///     
    ///     // Property that notifies the Ok command when changed
    ///     public string? Input { get => Get&lt;string?&gt;(); set => When(value).Notify(Ok).Set(); }
    ///     
    ///     // Command
    ///     public Command Ok => Do(() => MessageBox.Show(Input)).If(() => !string.IsNullOrWhiteSpace(Input));
    /// }
    /// </code>
    /// </example>
    /// </summary>
    public abstract class ViewModelBase : FluentSetterViewModelBase
    {
        /// <summary>
        /// Creates a fluent setter for a property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <returns>The fluent setter instance.</returns>
        protected FluentSetterBuilder<TValue> When<TValue>(TValue value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (GetFluentSetterBuilder(propertyName) is FluentSetterBuilder<TValue> exsistingBuilder)
            {
                exsistingBuilder.ValueToSet(value);
                return exsistingBuilder;
            }

            return new FluentSetterBuilder<TValue>(value, this, propertyName);
        }
    }
}