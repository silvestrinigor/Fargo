using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IArticleRepository
    {
        void Add(Article article);
        void Remove(Article article);
    }
}