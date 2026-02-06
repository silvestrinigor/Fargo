using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public abstract class EntityByGuidRepository<TEntity>(
            DbSet<TEntity> set
            ) : IEntityByGuidRepository<TEntity>
                where TEntity : class, IEntityByGuid, IEntityPartitioned
    {
        protected readonly DbSet<TEntity> dbSet = set;

        public async Task<TEntity?> GetByGuidAsync(
                Guid entityGuid,
                IReadOnlyCollection<Guid> partitionGuids,
                CancellationToken cancellationToken = default
                )
            => await GetByGuidAsync(
                    dbSet.AsQueryable(),
                    entityGuid,
                    partitionGuids,
                    cancellationToken
                    );

        protected static async Task<TEntity?> GetByGuidAsync(
                IQueryable<TEntity> query,
                Guid entityGuid,
                IReadOnlyCollection<Guid> partitionGuids,
                CancellationToken cancellationToken = default
                )
            => await query.SingleOrDefaultAsync(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid)),
                    cancellationToken
                    );
    }
}