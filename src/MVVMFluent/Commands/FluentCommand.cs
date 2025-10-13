using MVVMFluent.Interfaces;
using System;

namespace MVVMFluent.Commands;

/// <summary>
/// Represents a command that can be executed and has an associated execution condition.
/// </summary>
public class FluentCommand : IFluentCommand
{
    private Action<object?>? _execute;
    private Func<object?, bool>? _canExecute;
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the command has been fully configured and built.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Gets the view model that owns this command, if any.
    /// </summary>
    public IFluentSetterViewModel? Owner { get; private set; }

    /// <summary>
    /// Occurs when changes affecting the ability of the command to execute should be re-evaluated.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Marks the command as built, preventing further configuration changes.
    /// </summary>
    public void MarkAsBuilt() => IsBuilt = true;

    /// <summary>
    /// Sets the delegate that will be executed when the command runs.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    protected void SetCommand(Action<object?> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    /// <summary>
    /// Creates a new command that executes the specified delegate without a parameter.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="FluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    public static FluentCommand Do(Action execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new FluentCommand { Owner = owner };
        command.SetCommand(_ => execute());
        return command;
    }

    /// <summary>
    /// Creates a new command that executes the specified delegate with a parameter.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="FluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    public static FluentCommand Do(Action<object?> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new FluentCommand { Owner = owner };
        command.SetCommand(execute);
        return command;
    }

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand"/> instance.</returns>
    public FluentCommand If(Func<bool> canExecute) => If(_ => canExecute());

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/> for the given parameter.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canExecute"/> is <see langword="null"/>.</exception>
    public FluentCommand If(Func<object?, bool> canExecute)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (canExecute == null)
        {
            throw new ArgumentNullException(nameof(canExecute));
        }
        _canExecute = canExecute;
        return this;
    }

    /// <summary>
    /// Configures the command to only execute when the specified properties are free of validation errors.
    /// </summary>
    /// <param name="propertyNames">The property names that must be valid before the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand"/> instance.</returns>
    public FluentCommand IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    /// <summary>
    /// Determines whether the command can execute using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to evaluate.</param>
    /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Executes the command using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the command has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when no execution delegate has been configured.</exception>
    public void Execute(object? parameter)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FluentCommand));
        }

        if (_execute == null)
        {
            throw new InvalidOperationException("No execute action has been set for this command.");
        }

        if (CanExecute(parameter))
        {
            _execute.Invoke(parameter);
        }
    }

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the command's execution state may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private bool HasNoErrors(string[] propertyNames)
    {
        if (Owner is not IValidationFluentSetterViewModel viewModel)
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

    /// <summary>
    /// Releases the resources used by the command.
    /// </summary>
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _execute = null;
        _canExecute = null;
        CanExecuteChanged = null;
        Owner = null;
        _disposed = true;
    }
}

/// <summary>
/// Represents a command that can be executed with a parameter of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the parameter used by the command.</typeparam>
public class FluentCommand<T> : IFluentCommand<T>
{
    private Action<T?>? _execute;
    private Func<T?, bool>? _canExecute;
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the command has been fully configured and built.
    /// </summary>
    public bool IsBuilt { get; private set; }

    /// <summary>
    /// Gets the view model that owns this command, if any.
    /// </summary>
    public IFluentSetterViewModel? Owner { get; private set; }

    /// <summary>
    /// Occurs when changes affecting the ability of the command to execute should be re-evaluated.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Marks the command as built, preventing further configuration changes.
    /// </summary>
    public void MarkAsBuilt() => IsBuilt = true;

    /// <summary>
    /// Sets the delegate that will be executed when the command runs.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    protected void SetCommand(Action<T?> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

    /// <summary>
    /// Creates a new command that executes the specified delegate with a strongly-typed parameter.
    /// </summary>
    /// <param name="execute">The delegate to execute.</param>
    /// <param name="owner">The owning view model, if any.</param>
    /// <returns>A configured <see cref="FluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is <see langword="null"/>.</exception>
    public static FluentCommand<T> Do(Action<T?> execute, IFluentSetterViewModel? owner)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        var command = new FluentCommand<T> { Owner = owner };
        command.SetCommand(execute);
        return command;
    }

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/>.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand{T}"/> instance.</returns>
    public FluentCommand<T> If(Func<bool> canExecute) => If(_ => canExecute());

    /// <summary>
    /// Configures the command to only execute when the supplied predicate evaluates to <see langword="true"/> for the given parameter.
    /// </summary>
    /// <param name="canExecute">The predicate that determines whether the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="canExecute"/> is <see langword="null"/>.</exception>
    public FluentCommand<T> If(Func<T?, bool> canExecute)
    {
        if (IsBuilt)
        {
            return this;
        }

        if (canExecute == null)
        {
            throw new ArgumentNullException(nameof(canExecute));
        }
        _canExecute = canExecute;
        return this;
    }

    /// <summary>
    /// Configures the command to only execute when the specified properties are free of validation errors.
    /// </summary>
    /// <param name="propertyNames">The property names that must be valid before the command can execute.</param>
    /// <returns>The current <see cref="FluentCommand{T}"/> instance.</returns>
    public FluentCommand<T> IfValid(params string[] propertyNames)
    {
        EnsurePropertyNames(propertyNames);

        if (IsBuilt)
        {
            return this;
        }

        return If(() => HasNoErrors(propertyNames));
    }

    /// <summary>
    /// Determines whether the command can execute using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to evaluate.</param>
    /// <returns><see langword="true"/> if the command can execute; otherwise, <see langword="false"/>.</returns>
    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter is T typed ? typed : default) ?? true;
    }

    /// <summary>
    /// Executes the command using the supplied parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the execution delegate.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the command has already been disposed.</exception>
    public void Execute(object? parameter)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FluentCommand));
        }

        if (!CanExecute(parameter))
        {
            return;
        }

        _execute?.Invoke(parameter is T typed ? typed : default);
    }

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event to notify listeners that the command's execution state may have changed.
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private bool HasNoErrors(string[] propertyNames)
    {
        if (Owner is not IValidationFluentSetterViewModel viewModel)
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

    /// <summary>
    /// Releases the resources used by the command.
    /// </summary>
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _execute = null;
        _canExecute = null;
        CanExecuteChanged = null;
        Owner = null;
        _disposed = true;
    }
}
