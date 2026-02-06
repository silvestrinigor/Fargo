using Fargo.Application.Commom;
using Fargo.Application.Models;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Read.Repositories
{
    public abstract class EntityByGuidTemporalPartitionedReadRepository<TEntity>(
            DbSet<TEntity> set
            ) : IEntityByGuidTemporalPartitionedReadRepository<TEntity>
                where TEntity : class, IEntityByGuidReadModel, IEntityTemporalReadModel, IEntityPartitionedReadModel
    {
        protected readonly DbSet<TEntity> dbSet = set;

        public async Task<IEnumerable<TEntity>> GetManyAsync(
                IEnumerable<Guid> partitionGuids,
                DateTime? temporalAsOf = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await dbSet
            .TemporalAsOfIfDateTimeNotNull(temporalAsOf)
            .WithPagination(pagination);

        protected IQueryable<TEntity> TemporalAsOfIfNotNull(DateTime? temporalAsOf = null)
            => temporalAsOf is not null
            ? dbSet.TemporalAsOf(temporalAsOf.Value)
            : dbSet.AsQueryable();

        protected IQueryable<TEntity> WithPagination(Pagination pagination, IQueryable<TEntity> query)
            => query.Skip(pagination.Skip).Take(pagination.Limit);

        protected static async Task<IEnumerable<TEntity>> GetManyAsync(
                IQueryable<TEntity> query,
                IReadOnlyCollection<Guid> partitionGuids,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await GetManyAsync(
                    query.Skip(pagination.Skip).Take(pagination.Limit),
                    partitionGuids,
                    cancellationToken
                    );

        protected static async Task<IEnumerable<TEntity>> GetManyAsync(
                IQueryable<TEntity> query,
                IReadOnlyCollection<Guid> partitionGuids,
                CancellationToken cancellationToken = default
                )
            => await query
            .AsNoTracking()
            .Where(a => a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .ToListAsync(cancellationToken);

        public async Task<TEntity?> GetByGuidAsync(
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                DateTime? temporalAsOf = null,
                CancellationToken cancellationToken = default
                )
            => await GetByGuidAsync(
                    temporalAsOf is not null ? dbSet.TemporalAsOf(temporalAsOf.Value) : dbSet.AsQueryable(),
                    entityGuid,
                    partitionGuids,
                    cancellationToken
                    );

        protected static Task<TEntity?> GetByGuidAsync(
                IQueryable<TEntity> query,
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                CancellationToken cancellationToken = default
                )
            => query
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                    x.Guid == entityGuid &&
                    x.Partitions.Any(p => partitionGuids.Contains(p.Guid)),
                    cancellationToken
                    );
    }
}