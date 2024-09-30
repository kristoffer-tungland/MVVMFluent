namespace MVVMFluent
{
    internal static class TaskExtensions
    {
        internal async static void RunWithExceptionHandling(this System.Threading.Tasks.Task task, System.Action<System.Exception> onException, bool continueOnCapturedContext)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (System.Exception ex)
            {
                onException.Invoke(ex);
            }
        }
    }
}
