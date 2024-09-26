using MVVMFluent.WPF.Builders;
using MVVMFluent.WPF.Interfaces;

namespace MVVMFluent.WPF
{
    public static class CommandValidationExtensions
    {
        public static Command IfErrorFree(this Command command, IValidationFluentSetterViewModel viewModel, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            
            SubscribeToErrors(command, viewModel, propertyName);
            return command.If(() =>
            {
                return HasNoErrors(viewModel, propertyName);
            });
        }

        public static Command<T> IfErrorFree<T>(this Command<T> command, IValidationFluentSetterViewModel viewModel, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            
            SubscribeToErrors(command, viewModel, propertyName);
            return command.If(() =>
            {
                return HasNoErrors(viewModel, propertyName);
            });
        }

        public static AsyncCommand IfErrorFree(this AsyncCommand command, IValidationFluentSetterViewModel viewModel, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");

            SubscribeToErrors(command, viewModel, propertyName);
            return command.If(() =>
            {
                return HasNoErrors(viewModel, propertyName);
            });
        }

        public static AsyncCommand<T> IfErrorFree<T>(this AsyncCommand<T> command, IValidationFluentSetterViewModel viewModel, string? propertyName)
        {
            if (command.IsBuilt)
                return command;

            if (propertyName == null || string.IsNullOrWhiteSpace(propertyName))
                throw new global::System.ArgumentNullException(nameof(propertyName), "Property name cannot be null or empty.");
            SubscribeToErrors(command, viewModel, propertyName);
            return command.If(_ =>
            {
                return HasNoErrors(viewModel, propertyName);
            });
        }

        private static void SubscribeToErrors(IFluentCommand command, IValidationFluentSetterViewModel viewModel, string? propertyName)
        {
            viewModel.ErrorsChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                    command.RaiseCanExecuteChanged();
            };
        }

        private static bool HasNoErrors(IValidationFluentSetterViewModel viewModel, string propertyName)
        {
            var validationFluentSetter = viewModel.GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;
            return validationFluentSetter?.HasErrors == false;
        }
    }
}
