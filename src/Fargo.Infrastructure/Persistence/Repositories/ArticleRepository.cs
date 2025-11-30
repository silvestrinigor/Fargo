using Fargo.Domain.Entities;
using Fargo.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

internal class ArticleRepository(FargoContext fagoContext) : IArticleRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public void Add(Article article)
    {
        fargoContext.Articles.Add(article);
    }

    public async Task<Article?> GetByGuidAsync(Guid guid)
    {
        return await fargoContext.Articles
            .FirstOrDefaultAsync(a => a.Guid == guid);
    }

    public void Remove(Article article)
    {
        fargoContext.Articles.Remove(article);
    }
}
