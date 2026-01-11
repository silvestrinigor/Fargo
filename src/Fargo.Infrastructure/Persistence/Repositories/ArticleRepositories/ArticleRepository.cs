using Fargo.Domain.Entities;
using Fargo.Domain.Repositories.ArticleRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories.ArticleRepositories
{
    public class ArticleRepository(FargoContext context) : EntityByGuidRepository<Article>(context.Articles), IArticleRepository
    {
        private readonly FargoContext context = context;

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