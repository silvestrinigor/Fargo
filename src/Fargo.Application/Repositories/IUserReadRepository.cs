using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Repositories
{
    public interface IUserReadRepository : IEntityByGuidTemporalReadRepository<UserReadModel>
    {
        Task<IEnumerable<PermissionReadModel>?> GetUserPermissions(
            Guid userGuid,
            DateTime? asOfDateTime = null,
            CancellationToken cancellationToken = default);
    }
}
