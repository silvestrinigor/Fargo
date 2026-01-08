using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByGuidAsync(Guid userGuid, CancellationToken cancellationToken = default);

        void Add(User user);

        void Remove(User user);
    }
}
