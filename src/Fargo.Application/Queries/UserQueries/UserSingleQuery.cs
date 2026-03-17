using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Queries.UserQueries;

public sealed record UserSingleQuery(
        Guid UserGuid,
        DateTimeOffset? TemporalAsOf = null
        ) : IQuery<UserInformation?>;

public sealed class UserSingleQueryHandler(
        IUserRepository userRepository
        ) : IQueryHandler<UserSingleQuery, UserInformation?>
{
    public async Task<UserInformation?> Handle(
            UserSingleQuery query,
            CancellationToken cancellationToken = default
            )
    {
        return await userRepository.GetInfoByGuid(
                query.UserGuid,
                query.TemporalAsOf,
                cancellationToken
                );
    }
}
