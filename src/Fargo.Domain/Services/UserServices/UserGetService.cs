using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.UserServices
{
    public class UserGetService(
            IUserRepository userRepository
            )
    {
        public async Task<User?> GetUser(
                User actor,
                Guid userGuid,
                CancellationToken cancellationToken = default
                )
        {
            var user = await userRepository.GetByGuid(
                    userGuid,
                    partitionGuids: [.. actor.PartitionsAccesses.Select(p => p.Guid)],
                    cancellationToken
                    );

            return user;
        }
    }
}