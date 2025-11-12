using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

public class ArticleItemRepository(FargoContext fagoContext) : IArticleItemRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public async Task<ArticleItem?> GetAsync(Guid guid)
        => await fargoContext.ArticleItems.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<ArticleItem>> GetAsync()
        => await fargoContext.ArticleItems.ToListAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.ArticleItems.Select(x => x.Guid).ToListAsync();

    public void Add(ArticleItem articleItem)
        => fargoContext.ArticleItems.Add(articleItem);

    public void Remove(ArticleItem articleItem)
        => fargoContext.ArticleItems.Remove(articleItem);
}
