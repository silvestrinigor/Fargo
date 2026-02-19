namespace Fargo.Application.Requests.Queries
{
    public interface IQuery<out TResponse>;

    public interface IQueryHandler<in TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(
            TQuery query,
            CancellationToken cancellationToken = default
            );
    }
}