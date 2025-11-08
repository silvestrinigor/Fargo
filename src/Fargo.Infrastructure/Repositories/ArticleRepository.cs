using Fargo.Infrastructure.Contexts;
using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Fargo.Core.Contracts;

namespace Fargo.Infrastructure.Repositories;

internal class ArticleRepository(FargoContext fagoContext) : IArticleRepository
{
    private readonly FargoContext fargoContext = fagoContext;

    public void Add(Article article) => fargoContext.Articles.Add(article);

    public async Task<Article?> GetAsync(Guid guid) => await fargoContext.Articles.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Article>> GetAsync()
    {
        return await fargoContext.Articles.ToListAsync();
    }

    public void Remove(Article article)
    {
        fargoContext.Articles.Remove(article);
    }
}
