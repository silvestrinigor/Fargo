using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class UserRepository(FargoContext context) : IUserRepository
    {
        private readonly FargoContext context = context;

        public void Add(User user)
        {
            context.Users.Add(user);
        }

        public async Task<User?> GetByGuidAsync(
            Guid userGuid, 
            CancellationToken cancellationToken = default
            )
            => await context.Users
                .Include(x => x.Permissions)
                .Where(x => x.Guid == userGuid)
                .SingleOrDefaultAsync(cancellationToken);

        public void Remove(User user)
        {
            context.Users.Remove(user);
        }
    }
}
