using Fargo.Domain.Entities.Articles;

namespace Fargo.Domain.Repositories
{
    public interface IArticleRepository
    {
        Task<Article?> GetByGuidAsync(Guid guid);
        void Add(Article article);
        void Remove(Article article);
        Task<bool> HasItensAssociated(Guid articleGuid);
    }
}