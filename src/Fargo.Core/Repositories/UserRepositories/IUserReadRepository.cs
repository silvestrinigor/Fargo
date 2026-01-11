using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories.UserRepositories
{
    public interface IUserReadRepository : IEntityByGuidTemporalReadRepository<User>;
}
