using Fargo.Core.Entities;

namespace Fargo.Core.Contracts;

public interface IArticleRepository
{
    Task<Article?> GetAsync(Guid guid);
    Task<IEnumerable<Article>> GetAsync();
    void Add(Article area);
    void Remove(Article area);
}
