using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IEntityRepository
    {
        Task<Entity?> GetByGuidAsync(Guid guid);
    }
}
