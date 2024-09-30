namespace MVVMFluent
{
    public abstract class FluentSetterBuilderBase<TValue> : IFluentSetterBuilder, System.IDisposable
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

        public string GetPropertyName()
        {
            return FluentSetter.PropertyName;
        }

        public void ValueToSet(TValue? value)
        {
            _valueToSet = value;
        }

        public void Build()
        {
            if (IsBuilt)
                throw new System.InvalidOperationException("Fluent setter is already built.");

            _viewModel.AddFluentSetterBuilder(this);
            IsBuilt = true;
        }

        /// <summary>
        /// Sets the value and adds the fluent setter to the view model.
        /// </summary>
        public virtual void Set()
        {
            if (!IsBuilt)
                Build();

            FluentSetter.Set(_valueToSet);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private bool _disposed = false;
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _valueToSet = default;
                if (FluentSetter is System.IDisposable disposable)
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