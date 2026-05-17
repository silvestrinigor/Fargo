using Fargo.Core.Users;

namespace Fargo.Application.Users;

public interface IUserQueryRepository
{
    Task<UserDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<UserDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

public static class UserRepositoryExtensions
{
    extension(IUserRepository repository)
    {
        public async Task<User> GetFoundByGuid(
            Guid userGuid,
            CancellationToken cancellationToken = default
        )
        {
            var user = await repository.GetByGuid(userGuid, cancellationToken)
                ?? throw new UserNotFoundFargoApplicationException(userGuid);

            return user;
        }
    }
}
