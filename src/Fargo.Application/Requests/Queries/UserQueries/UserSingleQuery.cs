using Fargo.Application.Extensions;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.UserQueries
{
    public sealed record UserSingleQuery(
            Guid UserGuid,
            DateTime? TemporalAsOf = null
            ) : IQuery<UserResponseModel?>;

    public sealed class UserSingleQueryHandler(
            IUserReadRepository repository
            ) : IQueryHandler<UserSingleQuery, UserResponseModel?>
    {
        public async Task<UserResponseModel?> Handle(
                UserSingleQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var user = await repository.GetByGuid(
                    query.UserGuid,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return user?.ToResponse();
        }
    }
}