using System;
using System.Threading.Tasks;

namespace MVVMFluent;

/// <summary>
/// Provides helper extension methods for working with <see cref="Task"/> instances in view models.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Executes the specified task and forwards any thrown exception to the provided handler.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="handle">The handler that receives any exception thrown by the task.</param>
    /// <param name="continueOnCapturedContext">A value indicating whether to resume on the captured synchronization context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="task"/> or <paramref name="handle"/> is <see langword="null"/>.</exception>
    public static async void RunWithExceptionHandling(this Task task, Action<Exception> handle, bool continueOnCapturedContext)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        if (handle == null)
        {
            throw new ArgumentNullException(nameof(handle));
        }

        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex)
        {
            handle.Invoke(ex);
        }
    }
}
