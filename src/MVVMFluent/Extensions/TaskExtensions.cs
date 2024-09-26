namespace MVVMFluent.Extensions
{
    internal static class TaskExtensions
    {
        internal async static void RunWithExceptionHandling(this global::System.Threading.Tasks.Task task, global::System.Action<global::System.Exception> onException, bool continueOnCapturedContext)
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
