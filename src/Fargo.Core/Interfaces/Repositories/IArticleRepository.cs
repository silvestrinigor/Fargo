using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IArticleRepository
    {
        Task<Article?> GetByGuidAsync(Guid guid);
        void Add(Article article);
        void Remove(Article article);
    }
}