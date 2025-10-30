using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Queries;

/// <summary>
/// Dispatches queries to their matching handlers.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Sends the specified query to its handler and returns the produced result.
    /// </summary>
    /// <typeparam name="TResult">The type of result expected from the query.</typeparam>
    /// <param name="query">The query to dispatch.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that resolves to the result of the query.</returns>
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
