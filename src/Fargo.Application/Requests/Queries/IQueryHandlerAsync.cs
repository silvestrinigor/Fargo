namespace Fargo.Application.Requests.Queries
{
    /// <summary>
    /// Provides a query handler that returns a response.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public interface IQueryHandlerAsync<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> HandleAsync(
            TQuery query,
            CancellationToken cancellationToken = default
            );
    }
}