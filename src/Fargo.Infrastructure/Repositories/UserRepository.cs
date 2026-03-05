using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
    public class UserRepository(FargoWriteDbContext context) : IUserRepository
    {
        private readonly DbSet<User> users = context.Users;

        public Task<bool> Any(CancellationToken cancellationToken = default)
            => context.Users.AnyAsync(cancellationToken);

        public void Add(User user)
        {
            context.Users.Add(user);
        }

        public async Task<User?> GetByGuid(
                Guid entityGuid,
                CancellationToken cancellationToken = default
                )
            => await users
            .Include(u => u.UserPermissions)
            .Where(a =>
                    a.Guid == entityGuid
                  )
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<User?> GetByNameid(
                Nameid nameid,
                CancellationToken cancellationToken = default
                )
        {
            return await users.Where(a =>
                    a.Nameid == nameid
                    )
                .SingleOrDefaultAsync(cancellationToken);
        }

        public void Remove(User user)
        {
            context.Users.Remove(user);
        }
    }
}