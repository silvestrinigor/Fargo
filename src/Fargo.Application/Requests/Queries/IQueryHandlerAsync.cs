namespace Fargo.Application.Requests.Queries
{
    public interface IQueryHandlerAsync<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> HandleAsync(
            TQuery query,
            CancellationToken cancellationToken = default);
    }
}
