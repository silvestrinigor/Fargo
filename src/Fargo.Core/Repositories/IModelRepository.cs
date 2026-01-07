using Fargo.Domain.Entities.Models.Abstracts;

namespace Fargo.Domain.Repositories
{
    public interface IModelRepository
    {
        Task<Model?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

        void Add(Model model);

        void Remove(Model model);
    }
}
