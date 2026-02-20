using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories
{
    public class ArticleRepository(FargoWriteDbContext context)
        : IArticleRepository
    {
        private readonly DbSet<Article> articles = context.Articles;

        private readonly DbSet<Item> items = context.Items;

        public void Add(Article article)
            => articles.Add(article);

        public void Remove(Article article)
            => articles.Remove(article);

        public async Task<bool> HasItemsAssociated(
                Guid articleGuid,
                CancellationToken cancellationToken = default
                )
            => await items
            .Where(x => x.Article.Guid == articleGuid)
            .AnyAsync(cancellationToken);

        public async Task<Article?> GetByGuid(
                Guid entityGuid,
                IReadOnlyCollection<Guid>? partitionGuids = default,
                CancellationToken cancellationToken = default
                )
            => await articles
            .Where(a =>
                    a.Guid == entityGuid &&
                    a.Partitions.Any(p =>
                        partitionGuids == null ||
                        partitionGuids.Contains(p.Guid)
                        )
                  )
            .SingleOrDefaultAsync(cancellationToken);
    }
}