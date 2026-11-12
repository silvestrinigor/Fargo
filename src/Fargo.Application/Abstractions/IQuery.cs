namespace Fargo.Application.Abstractions;

public interface IQuery<out TResponse>;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default
    );
}
