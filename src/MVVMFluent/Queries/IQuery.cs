using System.Diagnostics.CodeAnalysis;

namespace MVVMFluent.Queries;

/// <summary>
/// Represents a marker interface for a query that produces a result of type <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the query.</typeparam>
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Queries are defined purely by their result type.")]
public interface IQuery<out TResult>
{
}
