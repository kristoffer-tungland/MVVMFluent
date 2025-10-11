using System;
using System.Windows.Input;

namespace MVVMFluent;

public interface IFluentCommand : ICommand, IDisposable
{
    void MarkAsBuilt();

    bool IsBuilt { get; }

    IFluentSetterViewModel? Owner { get; }

    void RaiseCanExecuteChanged();
}

/// <summary>
/// Represents a command that can be executed and has an associated execution condition.
/// </summary>
public class FluentCommand : IFluentCommand
{
    private Action<object?>? _execute;
    private Func<object?, bool>? _canExecute;
    private bool _disposed;

    public bool IsBuilt { get; private set; }

    public IFluentSetterViewModel? Owner { get; private set; }

    public event EventHandler? CanExecuteChanged;

    public void MarkAsBuilt() => IsBuilt = true;

    protected void SetCommand(Action<object?> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

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

    public FluentCommand If(Func<bool> canExecute) => If(_ => canExecute());

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

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

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

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

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
public class FluentCommand<T> : IFluentCommand
{
    private Action<T?>? _execute;
    private Func<T?, bool>? _canExecute;
    private bool _disposed;

    public bool IsBuilt { get; private set; }

    public IFluentSetterViewModel? Owner { get; private set; }

    public event EventHandler? CanExecuteChanged;

    public void MarkAsBuilt() => IsBuilt = true;

    protected void SetCommand(Action<T?> execute)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }
        _execute = execute;
    }

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

    public FluentCommand<T> If(Func<bool> canExecute) => If(_ => canExecute());

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

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter is T typed ? typed : default) ?? true;
    }

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

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

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
