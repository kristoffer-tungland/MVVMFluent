namespace MVVMFluent
{
    public interface IFluentSetterBuilder
    {
        /// <summary>
        /// Determines if the builder has been built.
        /// </summary>
        bool IsBuilt { get; }

        /// <summary>
        /// Builds the setter.
        /// </summary>
        void _intBuild();

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <returns>The property name.</returns>
        string GetPropertyName();
    }
}