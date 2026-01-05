using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleRepository(FargoContext context) : IArticleRepository
    {
        private readonly FargoContext context = context;

        public void Add(Article article)
        {
            context.Articles.Add(article);
        }

        public async Task<Article?> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        {
            return await context.Articles.FirstOrDefaultAsync(x => x.Guid == articleGuid, cancellationToken);
        }

        public async Task<bool> HasItensAssociated(Guid articleGuid, CancellationToken cancellationToken = default)
        {
            return await context.Items.Where(x => x.Article.Guid == articleGuid).AnyAsync(cancellationToken);
        }

        public void Remove(Article article)
        {
            context.Articles.Remove(article);
        }
    }
}
