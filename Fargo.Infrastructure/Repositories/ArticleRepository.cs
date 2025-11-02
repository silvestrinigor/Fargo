using Fargo.Infrastructure.Contexts;
using Fargo.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Fargo.Core.Contracts;

namespace Fargo.Infrastructure.Repositories;

internal class ArticleRepository(FagoContext fagoContext) : IArticleRepository
{
    private readonly FagoContext fagoContext = fagoContext;

    public void Add(Article article) => fagoContext.Articles.Add(article);

    public async Task<Article?> GetAsync(Guid guid) => await fagoContext.Articles.Where(x => x.Guid == guid).FirstOrDefaultAsync();

    public async Task<IEnumerable<Article>> GetAsync()
    {
        return await fagoContext.Articles.ToListAsync();
    }

    public void Remove(Article article)
    {
        fagoContext.Articles.Remove(article);
    }
}
