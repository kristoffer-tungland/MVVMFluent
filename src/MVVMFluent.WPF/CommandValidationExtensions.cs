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
        public static Command IfValid(this Command command, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            
            SubscribeToErrors(command, propertyName);
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
        public static Command<T> IfValid<T>(this Command<T> command, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            
            SubscribeToErrors(command, propertyName);
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
        public static AsyncCommand IfValid(this AsyncCommand command, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");

            SubscribeToErrors(command, propertyName);
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
        public static AsyncCommand<T> IfValid<T>(this AsyncCommand<T> command, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            SubscribeToErrors(command, propertyName);
            return command.If(_ =>
            {
                return HasNoErrors(command, propertyName);
            });
        }

        private static void SubscribeToErrors(IFluentCommand command, string? propertyName)
        {
            if (command.Owner is not IValidationFluentSetterViewModel viewModel)
                throw new global::System.ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");

            viewModel.ErrorsChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    command.RaiseCanExecuteChanged();
            };

            viewModel.CheckErrorsFor(propertyName);
        }

        private static bool HasNoErrors(IFluentCommand command, string propertyName)
        {
            if (command.Owner is not IValidationFluentSetterViewModel viewModel)
                throw new global::System.ArgumentNullException(nameof(viewModel), "ViewModel cannot be null.");

            var validationFluentSetter = viewModel.GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;
            return validationFluentSetter?.HasErrors == false;
        }
    }
}
