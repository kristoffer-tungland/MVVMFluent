namespace MVVMFluent.WPF
{
    public static class CommandValidationExtensions
    {
        /// <summary>
        /// Adds a condition to the command that checks if the property has no errors.
        /// </summary>
        /// <param name="command">The command to add the condition to.</param>
        /// <param name="propertyName">The name of the property to check for errors.</param>
        /// <returns>The command with the condition added.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        public static FluentCommand IfValid(this FluentCommand command, params string[] propertyName)
        {
            if (command.IsBuilt)
                return command;

            return command.If(() =>
            {
                return HasNoErrors(command, propertyName);
            });
        }

        /// <summary>
        /// Adds a condition to the command that checks if the property has no errors.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="command">The command to add the condition to.</param>
        /// <param name="propertyName">The name of the property to check for errors.</param>
        /// <returns>The command with the condition added.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        public static FluentCommand<T> IfValid<T>(this FluentCommand<T> command, params string[] propertyName)
        {
            if (command.IsBuilt)
                return command;
                        
            return command.If(() =>
            {
                return HasNoErrors(command, propertyName);
            });
        }

        /// <summary>
        /// Adds a condition to the command that checks if the property has no errors.
        /// </summary>
        /// <param name="command">The command to add the condition to.</param>
        /// <param name="propertyName">The name of the property to check for errors.</param>
        /// <returns>The command with the condition added.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        public static AsyncFluentCommand IfValid(this AsyncFluentCommand command, params string[] propertyName)
        {
            if (command.IsBuilt)
                return command;

            return command.If(() =>
            {
                return HasNoErrors(command, propertyName);
            });
        }

        /// <summary>
        /// Adds a condition to the command that checks if the property has no errors.
        /// </summary>
        /// <typeparam name="T">The type of the command parameter.</typeparam>
        /// <param name="command">The command to add the condition to.</param>
        /// <param name="propertyName">The name of the property to check for errors.</param>
        /// <returns>The command with the condition added.</returns>
        /// <exception cref="global::System.ArgumentNullException">Thrown when the property name is null or empty.</exception>
        public static AsyncFluentCommand<T> IfValid<T>(this AsyncFluentCommand<T> command, params string[] propertyName)
        {
            if (command.IsBuilt)
                return command;
                        
            return command.If(_ =>
            {
                return HasNoErrors(command, propertyName);
            });
        }

        private static bool HasNoErrors(IFluentCommand command, params string[] propertyName)
        {
            if (command.Owner is not IValidationFluentSetterViewModel viewModel)
                throw new global::System.ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");

            var hasErrors = false;

            foreach (var name in propertyName)
            {
                var validationFluentSetter = viewModel.GetFluentSetterBuilder(name) as IValidationFluentSetterBuilder;
                if (validationFluentSetter?.HasErrors == true)
                {
                    hasErrors = true;
                    break;
                }
            }

            return !hasErrors;
        }
    }
}
