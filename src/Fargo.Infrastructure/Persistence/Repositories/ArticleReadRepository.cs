using Fargo.Domain.Entities.Models;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleReadRepository(FargoContext context) : IArticleReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Article>> GetAllAsync(
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default
            )
        {
            var query = context.Articles.AsQueryable();

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<Article?> GetByGuidAsync(
            Guid articleGuid,
            DateTime? atDateTime = null,
            CancellationToken cancellationToken = default)
        {
            var query = atDateTime is not null
                ? context.Articles.TemporalAsOf(atDateTime.Value)
                : context.Articles;

            return await query
                .AsNoTracking()
                .Where(x => x.Guid == articleGuid)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
