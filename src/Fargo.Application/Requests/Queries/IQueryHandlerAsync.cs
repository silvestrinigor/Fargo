namespace Fargo.Application.Mediators
{
    public interface IQueryHandlerAsync<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> HandleAsync(
            TQuery query,
            CancellationToken cancellationToken = default);
    }
}
