using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;

namespace Fargo.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                );

        Task<User?> GetByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                );

        void Add(User user);

        void Remove(User user);

        Task<bool> Any(CancellationToken cancellationToken = default);
    }
}