using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public abstract class EntityByGuidRepository<TEntity>(DbSet<TEntity> set) : IEntityByGuidRepository<TEntity>
        where TEntity : class, IEntityByGuid
    {
        protected readonly DbSet<TEntity> dbSet = set;

        public async Task<TEntity?> GetByGuidAsync(
            Guid entityGuid,
            CancellationToken cancellationToken = default)
            => await GetByGuidAsync(
                dbSet.AsQueryable(),
                entityGuid,
                cancellationToken);

        protected static Task<TEntity?> GetByGuidAsync(
            IQueryable<TEntity> query,
            Guid entityGuid,
            CancellationToken cancellationToken = default)
            => query.SingleOrDefaultAsync(x => x.Guid == entityGuid, cancellationToken);
    }
}
