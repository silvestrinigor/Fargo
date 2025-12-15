using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Interfaces.Repositories
{
    public interface IEntityRepository
    {
        Task<Named?> GetByGuidAsync(Guid guid);
    }
}
