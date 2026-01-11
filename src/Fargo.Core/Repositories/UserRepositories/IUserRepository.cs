using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.UserRepositories
{
    public interface IUserRepository : IEntityByGuidRepository<User>
    {
        void Add(User user);

        void Remove(User user);
    }
}
