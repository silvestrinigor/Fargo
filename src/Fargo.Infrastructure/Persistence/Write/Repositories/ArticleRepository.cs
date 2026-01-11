using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Write.Repositories
{
    public class ArticleRepository(FargoWriteDbContext context) : EntityByGuidRepository<Article>(context.Articles), IArticleRepository
    {
        private readonly FargoWriteDbContext context = context;

        public void Add(Article article)
            => context.Articles.Add(article);

        public void Remove(Article article)
            => context.Articles.Remove(article);

        public async Task<bool> HasItensAssociated(
            Guid articleGuid,
            CancellationToken cancellationToken = default)
            => await context.Items
            .Where(x => x.Article.Guid == articleGuid)
            .AnyAsync(cancellationToken);
    }
}