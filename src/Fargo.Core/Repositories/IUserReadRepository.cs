using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    public interface IUserReadRepository
    {
        Task<User?> GetByGuidAsync(
            Guid userGuid, 
            DateTime? atDateTime = null, 
            CancellationToken cancellationToken = default
            );

        Task<IEnumerable<User>> GetAllAsync(
            DateTime? atDateTime = null, 
            int? skip = null, 
            int? take = null, 
            CancellationToken cancellationToken = default
            );
    }
}
