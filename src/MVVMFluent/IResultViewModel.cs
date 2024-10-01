namespace MVVMFluent
{
    /// <summary>
    /// Represents a ViewModel that returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IResultViewModel<TResult>
    {
        TResult GetResult();
    }
}
