using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Application.Requests.Queries.UserQueries
{
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
                    query.Pagination ?? Pagination.First20Pages,
                    query.TemporalAsOf,
                    cancellationToken
                    );
        }
    }
}