using Fargo.Domain.Users;

namespace Fargo.Application.Users;

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
