using Fargo.Domain.Entities.Models;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleReadRepository(FargoContext context) : IArticleReadRepository
    {
        private readonly FargoContext context = context;

        public async Task<IEnumerable<Article>> GetAllAsync(CancellationToken cancellationToken = default)
            => await context.Articles
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default)
            => await context.Articles
                .AsNoTracking()
                .Where(x => x.Guid == articleGuid)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<bool> HasItensAssociated(Guid articleGuid, CancellationToken cancellationToken = default)
            => await context.Items
                .AsNoTracking()
                .Where(x => x.Article.Guid == articleGuid)
                .AnyAsync(cancellationToken);
    }
}
