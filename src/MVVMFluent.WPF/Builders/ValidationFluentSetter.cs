using MVVMFluent.Builders;
using MVVMFluent.WPF.Interfaces;

namespace MVVMFluent.WPF.Builders
{
    public class ValidationFluentSetter<TValue> : FluentSetter<TValue>, IValidationFluentSetter<TValue>
    {
        private System.Collections.Generic.List<System.Windows.Controls.ValidationRule> _rules = new();
        public bool HasErrors { get; private set; }
        private readonly System.EventHandler<System.ComponentModel.DataErrorsChangedEventArgs>? _errorsChanged;

        public ValidationFluentSetter(IValidationFluentSetterViewModel viewModel, string? propertyName, System.EventHandler<System.ComponentModel.DataErrorsChangedEventArgs>? errorsChanged) :
            base(viewModel, propertyName)
        {
            _errorsChanged = errorsChanged;
        }

        public System.Collections.ObjectModel.ObservableCollection<string> Errors { get; } = new();

        public System.Collections.IEnumerable GetErrors()
        {
            return Errors;
        }

        internal ValidationFluentSetter<TValue> Validate(params System.Windows.Controls.ValidationRule[] rules)
        {
            foreach (var rule in rules)
            {
                if (_rules.Contains(rule))
                    continue;

                _rules.Add(rule);
            }
            return this;
        }

        internal ValidationFluentSetter<TValue> AddRule(System.Windows.Controls.ValidationRule rule)
        {
            if (_rules.Contains(rule))
                return this;
            _rules.Add(rule);
            return this;
        }

        public void CheckForErrors(TValue? valueToSet)
        {
            var hadErrors = HasErrors;
            Errors.Clear();
            HasErrors = false;

            foreach (var rule in _rules)
            {
                var validationResult = rule.Validate(valueToSet, System.Globalization.CultureInfo.CurrentCulture);

                if (!validationResult.IsValid)
                {
                    var errorMessage = validationResult.ErrorContent?.ToString();

                    if (errorMessage is null || string.IsNullOrWhiteSpace(errorMessage))
                        throw new System.InvalidOperationException("Validation rule did not return an error message.");

                    Errors.Add(errorMessage);
                    HasErrors = true;
                }
            }

            if (hadErrors != HasErrors)
                _errorsChanged?.Invoke(this, new System.ComponentModel.DataErrorsChangedEventArgs(PropertyName));
        }
    }
}
