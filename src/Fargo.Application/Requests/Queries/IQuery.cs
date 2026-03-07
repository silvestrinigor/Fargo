namespace Fargo.Application.Requests.Queries
{
    /// <summary>
    /// Represents a query in the application layer.
    ///
    /// Queries are used to retrieve data and must not change the state
    /// of the system.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The type of response returned by the query.
    /// </typeparam>
    public interface IQuery<out TResponse>;

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
        Task<TResponse> Handle(
            TQuery query,
            CancellationToken cancellationToken = default
        );
    }
}