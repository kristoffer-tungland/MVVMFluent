namespace MVVMFluent
{
    /// <summary>
    /// Base class for fluent setter builders.
    /// </summary>
    /// <typeparam name="TValue">The type of the value the FluentSetter will be used with.</typeparam>
    public abstract class FluentSetterBuilderBase<TValue> : IFluentSetterBuilder, global::System.IDisposable
    {
        protected TValue? _valueToSet;
        public bool IsBuilt { get; private set; }

        protected abstract IFluentSetter<TValue> FluentSetter { get; set; }
        private readonly IFluentSetterViewModel _viewModel;

        protected FluentSetterBuilderBase(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel)
        {
            _valueToSet = valueToSet;
            _viewModel = fluentSetterViewModel;
        }

        /// <summary>
        /// Gets the property name of underlying fluent setter.
        /// </summary>
        /// <returns>The property name.</returns>
        public string GetPropertyName()
        {
            return FluentSetter.PropertyName;
        }

        /// <summary>
        /// Sets the value to be set.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void ValueToSet(TValue? value)
        {
            _valueToSet = value;
        }

        /// <summary>
        /// Builds the fluent setter to make it ready to be used.
        /// </summary>
        /// <exception cref="global::System.InvalidOperationException">Thrown when the fluent setter is already built.</exception>
        public void Build()
        {
            if (IsBuilt)
                throw new global::System.InvalidOperationException("Fluent setter is already built.");

            _viewModel.AddFluentSetterBuilder(this);
            IsBuilt = true;
        }

        /// <summary>
        /// Commits the value change. This method is required to be called at the end of the fluent setter configuration.
        /// </summary>
        /// <remarks>When the value change it runs the configured logic on the fluent setter.</remarks>
        public virtual void Set()
        {
            if (!IsBuilt)
                Build();

            FluentSetter.Set(_valueToSet);
        }

        public void Dispose()
        {
            Dispose(true);
            global::System.GC.SuppressFinalize(this);
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _valueToSet = default;
                if (FluentSetter is global::System.IDisposable disposable)
                    disposable.Dispose();

                DisposeInternal();
            }

            _disposed = true;
        }

        protected virtual void DisposeInternal() { }

        ~FluentSetterBuilderBase()
        {
            Dispose(false);
        }
    }
}