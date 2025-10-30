using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Queries;

/// <summary>
/// Handles a query of type <typeparamref name="TQuery"/> and returns a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TQuery">The type of query handled.</typeparam>
/// <typeparam name="TResult">The type of result produced.</typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Processes the provided query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that resolves to the query result.</returns>
    Task<TResult> Handle(TQuery query, CancellationToken cancellationToken = default);
}
