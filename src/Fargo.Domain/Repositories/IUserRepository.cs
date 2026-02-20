using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        Task<User?> GetById(
                int entityId,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        void Add(User user);

        void Remove(User user);
    }
}