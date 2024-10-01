namespace MVVMFluent.WPF
{
    internal class ValidationFluentSetter<TValue> : FluentSetter<TValue>, IValidationFluentSetter<TValue>
    {
        private global::System.Collections.Generic.List<global::System.Windows.Controls.ValidationRule> _rules = new();
        public bool HasErrors { get; private set; }
        private readonly global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? _errorsChanged;
        private global::System.Func<TValue?, bool>? _validationFunction;
        private string? _errorMessage;

        public ValidationFluentSetter(IValidationFluentSetterViewModel viewModel, string? propertyName, global::System.EventHandler<global::System.ComponentModel.DataErrorsChangedEventArgs>? errorsChanged) :
            base(viewModel, propertyName)
        {
            _errorsChanged = errorsChanged;
        }

        public global::System.Collections.ObjectModel.ObservableCollection<string> Errors { get; } = new();

        public global::System.Collections.IEnumerable GetErrors()
        {
            return Errors;
        }

        internal ValidationFluentSetter<TValue> Validate(params global::System.Windows.Controls.ValidationRule[] rules)
        {
            foreach (var rule in rules)
            {
                if (_rules.Contains(rule))
                    continue;

                _rules.Add(rule);
            }
            return this;
        }

        internal ValidationFluentSetter<TValue> Validate(global::System.Func<TValue?, bool> validationFuntion, string? errorMessage)
        {
            _validationFunction = validationFuntion;
            _errorMessage = errorMessage;
            return this;
        }

        internal ValidationFluentSetter<TValue> AddRule(global::System.Windows.Controls.ValidationRule rule)
        {
            if (_rules.Contains(rule))
                return this;

            _rules.Add(rule);
            return this;
        }

        public override void Set(TValue? value)
        {
            CheckForErrors(value);
            base.Set(value);
        }

        /// <summary>
        /// Checks for errors based on the validation rules.
        /// </summary>
        /// <param name="valueToSet">Type of value to set.</param>
        /// <exception cref="global::System.InvalidOperationException">Thrown when the validation rule does not return an error message.</exception>
        public void CheckForErrors(TValue? valueToSet)
        {
            CheckForErrors((object?)valueToSet);
        }

        /// <summary>
        /// Checks for errors based on the validation rules.
        /// </summary>
        /// <param name="valueToSet">Value to set.</param>
        /// <exception cref="global::System.InvalidOperationException">Thrown when the validation rule does not return an error message.</exception>
        public void CheckForErrors(object? valueToSet)
        {
            var hadErrors = HasErrors;
            Errors.Clear();
            HasErrors = false;

            foreach (var rule in _rules)
            {
                var validationResult = rule.Validate(valueToSet, global::System.Globalization.CultureInfo.CurrentCulture);

                if (!validationResult.IsValid)
                {
                    var errorMessage = validationResult.ErrorContent?.ToString();

                    if (errorMessage is null || string.IsNullOrWhiteSpace(errorMessage))
                        throw new global::System.InvalidOperationException("Validation rule did not return an error message.");

                    Errors.Add(errorMessage);
                    HasErrors = true;
                }
            }

            if (_validationFunction is not null && !_validationFunction.Invoke((TValue?)valueToSet))
            {
                if (_errorMessage is null || string.IsNullOrWhiteSpace(_errorMessage))
                    _errorMessage = "Value is not valid.";

                Errors.Add(_errorMessage);
                HasErrors = true;
            }

            if (hadErrors != HasErrors)
                _errorsChanged?.Invoke(this, new global::System.ComponentModel.DataErrorsChangedEventArgs(PropertyName));
        }
    }
}
