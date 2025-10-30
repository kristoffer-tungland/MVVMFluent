using System;
using System.Threading;
using System.Threading.Tasks;

namespace MVVMFluent.Queries;

/// <summary>
/// Default <see cref="IQueryDispatcher"/> implementation that resolves handlers from an <see cref="IServiceProvider"/>.
/// </summary>
public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryDispatcher"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve query handlers.</param>
    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
        {
            throw new InvalidOperationException($"No query handler registered for '{query.GetType().FullName}' producing '{typeof(TResult).FullName}'.");
        }

        var handleMethod = handlerType.GetMethod(nameof(IQueryHandler<IQuery<object>, object>.Handle));

        if (handleMethod == null)
        {
            throw new InvalidOperationException($"The query handler for '{query.GetType().FullName}' does not expose a Handle method.");
        }

        var task = (Task<TResult>)handleMethod.Invoke(handler, new object[] { query, cancellationToken })!;
        return await task.ConfigureAwait(false);
    }
}
