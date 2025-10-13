using MVVMFluent.Interfaces;
using System;
using System.Windows.Input;

namespace MVVMFluent;

/// <summary>
/// Represents a command that can be executed with optional execution conditions.
/// Extends <see cref="ICommand"/> with builder lifecycle management and change notification.
/// </summary>
public interface IFluentCommand : ICommand, IDisposable
{
    /// <summary>
    /// Marks this command as built, preventing further configuration changes.
    /// </summary>
    void MarkAsBuilt();

    /// <summary>
    /// Gets a value indicating whether this command has been built and is ready for use.
    /// </summary>
    bool IsBuilt { get; }

    /// <summary>
    /// Gets the view model that owns this command, if any.
    /// </summary>
    IFluentSetterViewModel? Owner { get; }

    /// <summary>
    /// Raises the <see cref="ICommand.CanExecuteChanged"/> event to notify bindings that the command's executable state may have changed.
    /// </summary>
    void RaiseCanExecuteChanged();
}

/// <summary>
/// Represents a command that can be executed with a strongly-typed parameter and optional execution conditions.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public interface IFluentCommand<T> : IFluentCommand
{
}
