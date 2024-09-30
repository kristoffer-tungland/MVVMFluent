

namespace MVVMFluent.WPF
{
    public static class CommandValidationExtensions
    {
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
