namespace Fargo.Application;

/// <summary>
/// Defines a handler responsible for executing a query.
/// </summary>
/// <typeparam name="TQuery">
/// The type of query handled by this handler.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of response returned by the query.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Executes the specified query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The result produced by the query.
    /// </returns>
    Task<TResponse> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default
    );
}
