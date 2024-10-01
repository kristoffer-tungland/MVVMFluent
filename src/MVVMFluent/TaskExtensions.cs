using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent
{
    public static class TaskExtensions
    {
        public async static void RunWithExceptionHandling(this global::System.Threading.Tasks.Task task, global::System.Action<global::System.Exception> onException, bool continueOnCapturedContext)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (global::System.Exception ex)
            {
                onException.Invoke(ex);
            }
        }
    }
}