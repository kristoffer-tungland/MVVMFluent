namespace MVVMFluent
{
    /// <summary>
    /// Represents a builder for a fluent setter, see <see cref="FluentSetter{T}"/>.
    /// </summary>
    public class FluentSetterBuilder<TValue> : FluentSetterBuilderBase<TValue>, IFluentSetterBuilder
    {
        protected override IFluentSetter<TValue> FluentSetter { get; set; }

        public FluentSetterBuilder(TValue? valueToSet, IFluentSetterViewModel fluentSetterViewModel, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) :
            base(valueToSet, fluentSetterViewModel)
        {
            FluentSetter = new FluentSetter<TValue>(fluentSetterViewModel, propertyName);
        }

        private FluentSetter<TValue> GetFluentSetter()
        {
            return (FluentSetter<TValue>)FluentSetter;
        }

        /// <summary>
        /// Configures what happens before the value changes.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> OnChanging(global::System.Action<TValue?> action)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().OnChanging(action);
            return this;
        }

        /// <summary>
        /// Configures what happens before the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute before changing the value.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> OnChanging(global::System.Action<TValue?, TValue?> action)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().OnChanging(action);
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> OnChanged(global::System.Action<TValue?> action)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().OnChanged(action);
            return this;
        }

        /// <summary>
        /// Configures what happens after the value changes with old and new values.
        /// </summary>
        /// <param name="action">The action to execute after changing the value.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> OnChanged(global::System.Action<TValue?, TValue?> action)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().OnChanged(action);
            return this;
        }

        /// <summary>
        /// Specifies commands to reevaluate when the value changes.
        /// </summary>
        /// <param name="commands">The commands to reevaluate.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> Notify(params IFluentCommand[] commands)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().Notify(commands);
            return this;
        }

        /// <summary>
        /// Specifies properties to notify when the value changes.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to notify.</param>
        /// <returns>The current <see cref="FluentSetterBuilder{T}"/> instance.</returns>
        public FluentSetterBuilder<TValue> Notify(params string[] propertyNames)
        {
            if (IsBuilt)
                return this;

            GetFluentSetter().Notify(propertyNames);
            return this;
        }
    }
}