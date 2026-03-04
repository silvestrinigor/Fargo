using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;

namespace Fargo.Application.Repositories
{
    public interface IUserReadRepository
        {
            Task<UserReadModel?> GetByGuid(
                    Guid entityGuid,
                    DateTime? asOfDateTime = null,
                    CancellationToken cancellationToken = default
                    );

            Task<IReadOnlyCollection<UserReadModel>> GetMany(
                    DateTime? asOfDateTime = null,
                    Pagination pagination = default,
                    CancellationToken cancellationToken = default
                    );
        }
}