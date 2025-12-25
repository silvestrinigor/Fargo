using Fargo.Domain.Abstracts.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IEntityRepository
    {
        Task<Named?> GetByGuidAsync(Guid guid);
    }
}
