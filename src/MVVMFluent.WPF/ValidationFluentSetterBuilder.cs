namespace MVVMFluent.WPF
{
    public class ValidationFluentSetterBuilder<TValue> : FluentSetterBuilder<TValue>, IValidationFluentSetterBuilder
    {
        protected override IFluentSetter<TValue> FluentSetter { get; set; }

        public bool HasErrors => GetFluentSetter().HasErrors;

        public global::System.Collections.IEnumerable GetErrors()
        {
            return GetFluentSetter().GetErrors();
        }

        public ValidationFluentSetterBuilder(TValue value, IValidationFluentSetterViewModel fluentSetterViewModel, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null, global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? errorsChanged = null)
            : base(value, fluentSetterViewModel)
        {
            FluentSetter = new ValidationFluentSetter<TValue>(fluentSetterViewModel, propertyName, errorsChanged);
        }

        private ValidationFluentSetter<TValue> GetFluentSetter()
        {
            return (ValidationFluentSetter<TValue>)FluentSetter;
        }

        public ValidationFluentSetterBuilder<TValue> Validate(params global::System.Windows.Controls.ValidationRule[] rules)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().Validate(rules);
            return this;
        }

        public ValidationFluentSetterBuilder<TValue> Validate(global::System.Func<TValue?, bool> validationFuntion, string? errorMessage)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().Validate(validationFuntion, errorMessage);
            return this;
        }

        public ValidationFluentSetterBuilder<TValue> Required(string? errorMessage = default)
        {
            return Validate(new RequiredValidationRule(errorMessage));
        }

        /// <summary>
        /// Sets the value. This method is required to be called at the end of the fluent setter configuration.
        /// </summary>
        /// <remarks>This method runs the set action and checks for errors.</remarks>
        public override void Set()
        {
            base.Set();
        }

        public void CheckForErrors(object? value)
        {
            GetFluentSetter().CheckForErrors(value);
        }
    }
}
