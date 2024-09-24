namespace MVVMFluent
{
    internal class FluentSetter<T>
    {
        private readonly ViewModelBase _viewModel;
        private readonly string _propertyName;
        private readonly global::System.Collections.Generic.Dictionary<string, object?> _propertyStore;

        private T? _newValue;
        private T? _oldValue;

        private global::System.Action<T?>? _onChanging;
        private global::System.Action<T?, T?>? _onChangingOldNew;

        private global::System.Action<T?>? _onChanged;
        private global::System.Action<T?, T?>? _onChangedOldNew;

        private global::System.Collections.Generic.IEnumerable<IFluentCommand>? _commandsToReevaluate;
        private global::System.Collections.Generic.IEnumerable<string>? _propertiesToNotify;

        private bool _valueChanged = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentSetter{T}"/> class.
        /// </summary>
        /// <param name="viewModel">The view model to which the property belongs.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <param name="propertyStore">The backing store for the properties.</param>
        internal FluentSetter(ViewModelBase viewModel, string propertyName, global::System.Collections.Generic.Dictionary<string, object?> propertyStore)
        {
            _viewModel = viewModel;
            _propertyName = propertyName;
            _propertyStore = propertyStore;
        }

        /// <summary>
        /// Sets the new value and checks if it has changed.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> When(T newValue)
        {
            _newValue = newValue;

            // Get the old value from the backing store
            if (_propertyStore.TryGetValue(_propertyName, out var storedValue))
            {
                _oldValue = (T?)storedValue;
            }

            // Check if the value has changed
            _valueChanged = !global::System.Collections.Generic.EqualityComparer<T?>.Default.Equals(_oldValue, _newValue);
            return this;
        }

        /// <summary>
        /// Configures what happens before the value changes.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> OnChanging(global::System.Action<T?> action)
        {
            _onChanging = action;
            return this;
        }

        /// <summary>
        /// Configures what happens before the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> OnChanging(global::System.Action<T?, T?> action)
        {
            _onChangingOldNew = action;
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> OnChanged(global::System.Action<T?> action)
        {
            _onChanged = action;
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> OnChanged(global::System.Action<T?, T?> action)
        {
            _onChangedOldNew = action;
            return this;
        }

        /// <summary>
        /// Specifies commands to reevaluate when the value changes.
        /// </summary>
        /// <param name="commands">The commands to reevaluate.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> Notify(params IFluentCommand[] commands)
        {
            _commandsToReevaluate = commands;
            return this;
        }

        /// <summary>
        /// Specifies properties to notify when the value changes.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to notify.</param>
        /// <returns>The current <see cref="FluentSetter{T}"/> instance.</returns>
        internal FluentSetter<T> Notify(params string[] propertyNames)
        {
            _propertiesToNotify = propertyNames;
            return this;
        }

        /// <summary>
        /// Commits the value change and runs the configured logic.
        /// </summary>
        internal void Set()
        {
            if (_valueChanged)
            {
                // Trigger OnChanging actions
                _onChanging?.Invoke(_newValue);
                _onChangingOldNew?.Invoke(_oldValue, _newValue);

                // Update the backing store
                _propertyStore[_propertyName] = _newValue;

                // Trigger OnChanged actions
                _onChanged?.Invoke(_newValue);
                _onChangedOldNew?.Invoke(_oldValue, _newValue);

                // Reevaluate commands if necessary
                if (_commandsToReevaluate != null)
                {
                    foreach (var command in _commandsToReevaluate)
                    {
                        command.RaiseCanExecuteChanged();
                    }
                }

                // Notify property change
                _viewModel.OnPropertyChanged(_propertyName);

                // Notify other property changes
                if (_propertiesToNotify != null)
                {
                    foreach (var propertyName in _propertiesToNotify)
                    {
                        _viewModel.OnPropertyChanged(propertyName);
                    }
                }
            }
        }
    }
}