using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleReadRepository(
            FargoReadDbContext context
            ) : IArticleReadRepository
    {
        private readonly DbSet<ArticleReadModel> articles = context.Articles;

        public async Task<ArticleReadModel?> GetByGuid(
                Guid entityGuid,
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                CancellationToken cancellationToken = default
                )
            => await articles
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyCollection<ArticleReadModel>> GetMany(
                IEnumerable<Guid> partitionGuids,
                DateTime? asOfDateTime = null,
                Pagination pagination = default,
                CancellationToken cancellationToken = default
                )
            => await articles
            .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
            .Where(a => a.Partitions.Any(p => partitionGuids.Contains(p.Guid)))
            .WithPagination(pagination)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}