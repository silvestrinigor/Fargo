using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Repositories
{
    public interface IUserReadRepository
        {
            Task<UserReadModel?> GetByGuid(
                    Guid entityGuid,
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<UserReadModel>> GetMany(
                    IEnumerable<Guid> partitionGuids,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<PermissionReadModel>?> GetUserPermissions(
                    Guid userGuid,
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );
        }
}