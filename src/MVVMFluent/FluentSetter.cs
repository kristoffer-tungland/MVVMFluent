namespace MVVMFluent
{
    public class FluentSetter<TValue> : IFluentSetter<TValue>, global::System.IDisposable
    {
        protected readonly IFluentSetterViewModel _viewModel;

        protected global::System.Action<TValue?>? _onChanging;
        protected global::System.Action<TValue?, TValue?>? _onChangingOldNew;

        protected global::System.Action<TValue?>? _onChanged;
        protected global::System.Action<TValue?, TValue?>? _onChangedOldNew;

        protected global::System.Collections.Generic.IEnumerable<IFluentCommand>? _commandsToReevaluate;
        protected global::System.Collections.Generic.IEnumerable<string>? _propertiesToNotify;

        public string PropertyName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentSetter{T}"/> class.
        /// </summary>
        /// <param name="viewModel">The view model to which the property belongs thats implementing <see cref="IFluentSetterViewModel"/>.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        public FluentSetter(IFluentSetterViewModel viewModel, string? propertyName)
        {
            if (propertyName == null)
                throw new global::System.ArgumentNullException(nameof(propertyName), "Not able to determine property name to set.");

            PropertyName = propertyName;
            _viewModel = viewModel;
        }

        /// <summary>
        /// Configures what happens before the value changes.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changing(global::System.Action action)
        {
            _onChanging = _ => action();
            return this;
        }

        /// <summary>
        /// Configures what happens before the value changes.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changing(global::System.Action<TValue?> action)
        {
            _onChanging = action;
            return this;
        }

        /// <summary>
        /// Configures what happens before the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changing(global::System.Action<TValue?, TValue?> action)
        {
            _onChangingOldNew = action;
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changed(global::System.Action action)
        {
            _onChanged = _ => action();
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changed(global::System.Action<TValue?> action)
        {
            _onChanged = action;
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Changed(global::System.Action<TValue?, TValue?> action)
        {
            _onChangedOldNew = action;
            return this;
        }

        /// <summary>
        /// Specifies commands to reevaluate when the value changes.
        /// </summary>
        /// <param name="commands">The commands to reevaluate.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Notify(params IFluentCommand[] commands)
        {
            _commandsToReevaluate = commands;
            return this;
        }

        /// <summary>
        /// Specifies properties to notify when the value changes.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to notify.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<TValue> Notify(params string[] propertyNames)
        {
            _propertiesToNotify = propertyNames;
            return this;
        }

        /// <summary>
        /// Commits the value change and runs the configured logic.
        /// </summary>
        public virtual void Set(TValue? value)
        {
            // Get the old value from the backing store
            var oldValue = _viewModel.GetFieldValue<TValue>(PropertyName);

            // Check if the value has changed
            var valueHasChanged = !global::System.Collections.Generic.EqualityComparer<TValue?>.Default.Equals(oldValue, value);

            if (!valueHasChanged)
                return;

            // Trigger Changing actions
            _onChanging?.Invoke(value);
            _onChangingOldNew?.Invoke(oldValue, value);

            // Update the backing store
            _viewModel.SetFieldValue(PropertyName, value);

            // Trigger Changed actions
            _onChanged?.Invoke(value);
            _onChangedOldNew?.Invoke(oldValue, value);

            // Reevaluate commands if necessary
            if (_commandsToReevaluate != null)
            {
                foreach (var command in _commandsToReevaluate)
                {
                    command.RaiseCanExecuteChanged();
                }
            }

            // Notify property change
            _viewModel.OnPropertyChanged(PropertyName);

            // Notify other property changes
            if (_propertiesToNotify != null)
            {
                foreach (var propertyName in _propertiesToNotify)
                {
                    _viewModel.OnPropertyChanged(propertyName);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _onChanging = null;
                _onChangingOldNew = null;
                _onChanged = null;
                _onChangedOldNew = null;
                _commandsToReevaluate = null;
                _propertiesToNotify = null;
            }
        }

        ~FluentSetter()
        {
            Dispose(false);
        }
    }
}