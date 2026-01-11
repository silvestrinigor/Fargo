using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public abstract class EntityByGuidTemporalReadRepository<TEntity>(DbSet<TEntity> set) : IEntityByGuidTemporalReadRepository<TEntity>
        where TEntity : class, IEntityByGuid, IEntityTemporal
    {
        protected readonly DbSet<TEntity> dbSet = set;

        public async Task<IEnumerable<TEntity>> GetManyAsync(
            DateTime? atDateTime = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetManyAsync(
                atDateTime is not null ? dbSet.TemporalAsOf(atDateTime.Value) : dbSet.AsQueryable(),
                pagination,
                cancellationToken);

        protected static async Task<IEnumerable<TEntity>> GetManyAsync(
            IQueryable<TEntity> query,
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetManyAsync(
                query.Skip(pagination.Skip).Take(pagination.Limit),
                cancellationToken);

        protected static async Task<IEnumerable<TEntity>> GetManyAsync(
            IQueryable<TEntity> query,
            CancellationToken cancellationToken = default)
            => await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<TEntity?> GetByGuidAsync(
            Guid entityGuid,
            DateTime? atDateTime = null,
            CancellationToken cancellationToken = default)
            => await GetByGuidAsync(
                atDateTime is not null ? dbSet.TemporalAsOf(atDateTime.Value) : dbSet.AsQueryable(), 
                entityGuid, 
                cancellationToken);

        protected static Task<TEntity?> GetByGuidAsync(
            IQueryable<TEntity> query,
            Guid entityGuid,
            CancellationToken cancellationToken = default)
            => query
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Guid == entityGuid, cancellationToken);
    }
}
