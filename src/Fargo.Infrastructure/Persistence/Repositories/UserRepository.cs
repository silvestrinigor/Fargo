using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class UserRepository(FargoWriteDbContext context) : IUserRepository
    {
        private readonly DbSet<User> users = context.Users;

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

        public async Task<User?> GetById(
                int entityId,
                IReadOnlyCollection<Guid>? partitionGuids = null,
                CancellationToken cancellationToken = default
                )
            => await users
            .Where(a =>
                    a.Partitions.Any(p =>
                        partitionGuids == null ||
                        partitionGuids.Contains(p.Guid)
                        )
                  )
            .SingleOrDefaultAsync(cancellationToken);

        public Task<User?> GetByNameid(Nameid nameid, IReadOnlyCollection<Guid>? partitionGuids = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(User user)
        {
            context.Users.Remove(user);
        }
    }
}