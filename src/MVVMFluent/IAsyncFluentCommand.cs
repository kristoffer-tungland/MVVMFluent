using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent;

/// <summary>
/// Represents an asynchronous command that supports cancellation, progress reporting, and execution state tracking.
/// </summary>
public interface IAsyncFluentCommand : IFluentCommand
{
    /// <summary>
    /// Gets a value indicating whether the command is currently executing.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets or sets the current progress percentage (0-100) of the command execution.
    /// </summary>
    int Progress { get; set; }

    /// <summary>
    /// Reports progress as a percentage value (0-100).
    /// </summary>
    /// <param name="progress">The progress percentage.</param>
    void ReportProgress(int progress);

    /// <summary>
    /// Reports progress by calculating the percentage from current and total values.
    /// </summary>
    /// <param name="current">The current progress value.</param>
    /// <param name="total">The total progress value.</param>
    void ReportProgress(int current, int total);

    /// <summary>
    /// Gets a command that can be used to cancel the currently executing asynchronous operation.
    /// The cancel command is only enabled when the async command is running and not already cancelled.
    /// </summary>
    IFluentCommand CancelCommand { get; }

    /// <summary>
    /// Gets a value indicating whether cancellation has been requested for the current operation.
    /// </summary>
    bool IsCancellationRequested { get; }

    /// <summary>
    /// Gets the cancellation token source for the currently executing operation, or null if no operation is running.
    /// </summary>
    CancellationTokenSource? CancellationTokenSource { get; }

    /// <summary>
    /// Executes the asynchronous command with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the command execution logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(object? parameter);

    /// <summary>
    /// Requests cancellation of the currently executing operation.
    /// </summary>
    void Cancel();
}

/// <summary>
/// Represents an asynchronous command with a strongly-typed parameter that supports cancellation, progress reporting, and execution state tracking.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public interface IAsyncFluentCommand<T> : IAsyncFluentCommand
{
    /// <summary>
    /// Executes the asynchronous command with the specified strongly-typed parameter.
    /// </summary>
    /// <param name="parameter">The parameter to pass to the command execution logic.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteAsync(T parameter);
}