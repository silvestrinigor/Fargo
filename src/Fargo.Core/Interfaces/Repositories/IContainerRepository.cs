using Fargo.Domain.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IContainerRepository
    {
        Task<Container?> GetContainerAsync(Guid guid);
        void AddContainer(Container entity);
        void RemoveContainer(Container entity);
        void InsertIntoContainer(Container container, ArticleItem item);
        void RemoveFromContainer(Container container, ArticleItem item);
    }
}