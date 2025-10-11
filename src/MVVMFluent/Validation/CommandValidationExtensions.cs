using System;

namespace MVVMFluent;

public static class CommandValidationExtensions
{
    public static FluentCommand IfValid(this FluentCommand command, params string[] propertyNames) =>
        IfValidCore(command, propertyNames, c => c.If(() => HasNoErrors(c, propertyNames)));

    public static FluentCommand<T> IfValid<T>(this FluentCommand<T> command, params string[] propertyNames) =>
        IfValidCore(command, propertyNames, c => c.If(() => HasNoErrors(c, propertyNames)));

    public static AsyncFluentCommand IfValid(this AsyncFluentCommand command, params string[] propertyNames) =>
        IfValidCore(command, propertyNames, c => c.If(() => HasNoErrors(c, propertyNames)));

    public static AsyncFluentCommand<T> IfValid<T>(this AsyncFluentCommand<T> command, params string[] propertyNames) =>
        IfValidCore(command, propertyNames, c => c.If(() => HasNoErrors(c, propertyNames)));

    private static TCommand IfValidCore<TCommand>(TCommand command, string[] propertyNames, Func<TCommand, TCommand> applyPredicate)
        where TCommand : class, IFluentCommand
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        if (applyPredicate == null)
        {
            throw new ArgumentNullException(nameof(applyPredicate));
        }

        EnsurePropertyNames(propertyNames);

        if (command.IsBuilt)
        {
            return command;
        }

        return applyPredicate(command);
    }

    private static bool HasNoErrors(IFluentCommand command, string[] propertyNames)
    {
        if (command.Owner is not IValidationFluentSetterViewModel viewModel)
        {
            throw new InvalidOperationException(
                "Validation commands require a view model derived from ValidationViewModelBase.");
        }

        foreach (var propertyName in propertyNames)
        {
            var builder = viewModel.GetFluentSetterBuilder(propertyName) as IValidationFluentSetterBuilder;
            if (builder?.HasErrors == true)
            {
                return false;
            }
        }

        return true;
    }

    private static void EnsurePropertyNames(string[] propertyNames)
    {
        if (propertyNames == null)
        {
            throw new ArgumentNullException(nameof(propertyNames));
        }

        if (propertyNames.Length == 0)
        {
            throw new ArgumentException("At least one property name must be provided.", nameof(propertyNames));
        }

        foreach (var propertyName in propertyNames)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("Property names cannot be null or whitespace.", nameof(propertyNames));
            }
        }
    }
}
