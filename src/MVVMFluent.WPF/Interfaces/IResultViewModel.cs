namespace MVVMFluent;

/// <summary>
/// Represents a ViewModel that returns a result.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IResultViewModel<TResult>
{
    /// <summary>
    /// Gets the result produced by the view model after the dialog completes.
    /// </summary>
    /// <returns>The value that represents the outcome of the dialog interaction.</returns>
    TResult GetResult();
}
