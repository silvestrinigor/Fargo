using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleRepository(FargoContext context) : IArticleRepository
    {
        private readonly FargoContext context = context;

        public void Add(Article article)
            => context.Articles.Add(article);

        public void Remove(Article article)
            => context.Articles.Remove(article);

        public async Task<Article?> GetByGuidAsync(
            Guid articleGuid, 
            CancellationToken cancellationToken = default
            )
            => await context.Articles
                .Where(x => x.Guid == articleGuid)
                .SingleOrDefaultAsync(cancellationToken
                );

        public async Task<bool> HasItensAssociated(
            Guid articleGuid, 
            CancellationToken cancellationToken = default
            )
            => await context.Items
                .Where(x => x.Article.Guid == articleGuid)
                .AnyAsync(cancellationToken);
    }
}