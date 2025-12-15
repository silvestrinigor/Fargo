using Fargo.Domain.Entities.Articles;
using Fargo.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

internal class ArticleRepository(FargoContext fagoContext) : IArticleRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public void Add(ItemArticle article)
    {
        fargoContext.Articles.Add(article);
    }

    public async Task<ItemArticle?> GetByGuidAsync(Guid guid)
    {
        return await fargoContext.Articles
            .FirstOrDefaultAsync(a => a.Guid == guid);
    }

    public void Remove(ItemArticle article)
    {
        fargoContext.Articles.Remove(article);
    }

    public Task<bool> HasItensAssociated(Guid articleGuid)
    {
        return fargoContext.Items.AnyAsync(i => i.ArticleGuid == articleGuid);
    }
}
