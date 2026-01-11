using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.UserRepositories;

namespace Fargo.Infrastructure.Persistence.Repositories.UserRepositories
{
    public class UserRepository(FargoContext context) : EntityByGuidRepository<User>(context.Users), IUserRepository
    {
        private readonly FargoContext context = context;

        public void Add(User user)
        {
            context.Users.Add(user);
        }

        public void Remove(User user)
        {
            context.Users.Remove(user);
        }
    }
}
