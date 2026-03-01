using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.UserServices
{
    public class ActorGetService(
            IUserRepository userRepository
            )
    {
        public async Task<User?> GetActor(
                Guid userGuid,
                CancellationToken cancellationToken = default
                )
        {
            var user = await userRepository.GetByGuid(
                    userGuid,
                    partitionGuids: null,
                    cancellationToken
                    );

            return user;
        }
    }
}