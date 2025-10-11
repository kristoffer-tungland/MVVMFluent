using global::System.Linq;

namespace MVVMFluent
{
    /// <summary>
    /// Represents a base class for view models that provides property change notification, command creation, and validation.
    /// </summary>
    public abstract class ValidationViewModelBase : FluentSetterViewModelBase, IValidationFluentSetterViewModel
    {
        public bool HasErrors { get; private set; }

        public global::System.Collections.ObjectModel.ObservableCollection<string> Errors { get; } = new();

        public event global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? ErrorsChanged;

        protected ValidationViewModelBase()
        {
            ErrorsChanged += ErrorsChangedHandler;
        }

        private void ErrorsChangedHandler(object? sender, global::System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            Errors.Clear();
            var builders = _builderStore.Values.OfType<IValidationFluentSetterBuilder>().ToList();

            foreach (var builder in builders)
            {
                var errors = builder.GetErrors().OfType<string>().ToList();

                if (!errors.Any())
                    continue;

                var propertyName = builder.GetPropertyName();

                Errors.Add($"{propertyName}: {string.Join(", ", errors)}");
            }

            HasErrors = builders.Any(x => x.HasErrors);
        }

        public global::System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                return GetAllErrors();

            var validationFluentSetter = GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;

            if (validationFluentSetter == null)
                return global::System.Array.Empty<string>();

            return validationFluentSetter.GetErrors();
        }

        /// <summary>
        /// Checks for errors on a property.
        /// </summary>
        /// <param name="propertyName">Name of the property to check for errors.</param>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        /// <exception cref="global::System.ArgumentException">Thrown when the property is not found on the view model.</exception>
        public void CheckErrorsFor(string? propertyName)
        {
            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");

            var validationFluentSetter = GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;

            if (validationFluentSetter == null)
                return;

            var property = GetType().GetProperty(propertyName);
            var value = property?.GetValue(this);

            validationFluentSetter.CheckForErrors(value);
        }

        private global::System.Collections.IEnumerable GetAllErrors()
        {
            return _builderStore.Values.OfType<IValidationFluentSetterBuilder>().Select(x => x.GetErrors());
        }

        /// <summary>
        /// Creates a fluent setter for a property with validation.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="value">The new value to set.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <returns>The fluent setter instance.</returns>
        protected ValidationFluentSetterBuilder<TValue> When<TValue>(TValue value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            if (GetFluentSetterBuilder(propertyName) is ValidationFluentSetterBuilder<TValue> exsistingBuilder)
            {
                exsistingBuilder._intValueToSet(value);
                return exsistingBuilder;
            }

            return new ValidationFluentSetterBuilder<TValue>(value, this, propertyName, ErrorsChanged);
        }

        protected override void DisposeInternal()
        {
            ErrorsChanged -= ErrorsChangedHandler;
            ErrorsChanged = null;

            base.DisposeInternal();
        }
    }
}
