using Fargo.Domain.Entities.Articles;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IArticleRepository
    {
        Task<PhysicalProductArticle?> GetByGuidAsync(Guid guid);
        void Add(PhysicalProductArticle article);
        void Remove(PhysicalProductArticle article);
        Task<bool> HasItensAssociated(Guid articleGuid);
    }
}