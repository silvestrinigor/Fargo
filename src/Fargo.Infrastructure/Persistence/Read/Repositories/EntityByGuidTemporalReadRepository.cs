using Fargo.Application.Commom;
using Fargo.Application.Models;
using Fargo.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public abstract class EntityByGuidTemporalReadRepository<TEntity>(
        DbSet<TEntity> set
        ) : IEntityByGuidTemporalReadRepository<TEntity>
        where TEntity : class, IEntityByGuidReadModel, IEntityTemporalReadModel
    {
        protected readonly DbSet<TEntity> dbSet = set;

        public async Task<IEnumerable<TEntity>> GetManyAsync(
            DateTime? temporalAsOf = null,
            Pagination pagination = default,
            CancellationToken cancellationToken = default)
            => await GetManyAsync(
                temporalAsOf is not null ? dbSet.TemporalAsOf(temporalAsOf.Value) : dbSet.AsQueryable(),
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
            DateTime? temporalAsOf = null,
            CancellationToken cancellationToken = default)
            => await GetByGuidAsync(
                temporalAsOf is not null ? dbSet.TemporalAsOf(temporalAsOf.Value) : dbSet.AsQueryable(),
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
