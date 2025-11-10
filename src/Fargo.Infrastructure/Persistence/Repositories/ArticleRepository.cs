using Fargo.Core.Contracts;
using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fargo.Infrastructure.Persistence.Repositories;

internal class ArticleRepository(FargoContext fagoContext) : IArticleRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public async Task<Article?> GetAsync(Guid guid)
        => await fargoContext.Articles.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Article>> GetAsync()
        => await fargoContext.Articles.ToListAsync();

    public async Task<IEnumerable<Guid>> GetGuidsAsync()
        => await fargoContext.Articles.Select(x => x.Guid).ToListAsync();

    public void Add(Article article)
        => fargoContext.Articles.Add(article);

    public void Remove(Article article)
        => fargoContext.Articles.Remove(article);
}
