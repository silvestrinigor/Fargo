using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public class UserRepository(FargoWriteDbContext context) : EntityByGuidRepository<User>(context.Users), IUserRepository
    {
        private readonly FargoWriteDbContext context = context;

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
