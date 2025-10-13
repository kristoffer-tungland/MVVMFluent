using System;
using System.Threading.Tasks;

namespace MVVMFluent;

public static class TaskExtensions
{
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
