using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        Task<User?> GetByNameid(
                Nameid nameid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                );

        void Add(User user);

        void Remove(User user);
    }
}