using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
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
                IReadOnlyCollection<Guid>? partitionGuids = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p =>
                        partitionGuids == null ||
                        partitionGuids.Contains(p.Guid)
                        )
                  )
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<User?> GetByNameid(Nameid nameid, IReadOnlyCollection<Guid>? partitionGuids = null, CancellationToken cancellationToken = default)
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