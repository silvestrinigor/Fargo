using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.UserRepositories;

namespace Fargo.Infrastructure.Persistence.Repositories.UserRepositories
{
    public class UserReadRepository(FargoContext context) : EntityByGuidTemporalReadRepository<User>(context.Users), IUserReadRepository;
}
