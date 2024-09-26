using MVVMFluent.Builders;
using MVVMFluent.WPF.Interfaces;
using MVVMFluent.WPF.ValidationRules;

namespace MVVMFluent.WPF.Builders
{

    public class ValidationFluentSetterBuilder<TValue> : FluentSetterBuilder<TValue>, IValidationFluentSetterBuilder
    {
        protected override IFluentSetter<TValue> FluentSetter { get; set; }

        public bool HasErrors => GetFluentSetter().HasErrors;

        public global::System.Collections.IEnumerable GetErrors()
        {
            return GetFluentSetter().GetErrors();
        }

        public ValidationFluentSetterBuilder(TValue value, IValidationFluentSetterViewModel fluentSetterViewModel, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null, System.EventHandler<System.ComponentModel.DataErrorsChangedEventArgs>? errorsChanged = null)
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

        public ValidationFluentSetterBuilder<TValue> Required(string? errorMessage = default)
        {
            return Validate(new RequiredValidationRule(errorMessage));
        }

        public override void Set()
        {
            base.Set();
            
            GetFluentSetter().CheckForErrors(_valueToSet);
        }
    }
}
