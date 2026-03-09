using Fargo.Application.Commom;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Repositories
{
        public class ArticleReadRepository(
                FargoReadDbContext context
                ) : IArticleReadRepository
        {
                private readonly DbSet<ArticleReadModel> articles = context.Articles;

                public async Task<ArticleReadModel?> GetByGuid(
                        Guid entityGuid,
                        DateTimeOffset? asOfDateTime = null,
                        CancellationToken cancellationToken = default
                        )
                    => await articles
                    .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
                    .Where(a => a.Guid == entityGuid)
                    .OrderBy(x => x.Guid)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(cancellationToken);

                public async Task<IReadOnlyCollection<ArticleReadModel>> GetMany(
                        DateTimeOffset? asOfDateTime = null,
                        Pagination pagination = default,
                        CancellationToken cancellationToken = default
                        )
                    => await articles
                    .TemporalAsOfIfDateTimeNotNull(asOfDateTime)
                    .WithPagination(pagination)
                    .OrderBy(x => x.Guid)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
        }
}