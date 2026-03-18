using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserQueries;

public sealed record UserManyQuery(
        DateTimeOffset? TemporalAsOf = null,
        Pagination? Pagination = null
        ) : IQuery<IReadOnlyCollection<UserInformation>>;

public sealed class UserManyQueryHandler(
        IUserRepository userRepository
        ) : IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>>
{
    public async Task<IReadOnlyCollection<UserInformation>> Handle(
            UserManyQuery query,
            CancellationToken cancellationToken = default
            )
    {
        return await userRepository.GetManyInfo(
                query.Pagination ?? Pagination.FirstPage20Items,
                query.TemporalAsOf,
                cancellationToken
                );
    }
}
