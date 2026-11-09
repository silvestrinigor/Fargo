using Fargo.Application.Operations.UserGroups;
using Fargo.Application.Common;
using Fargo.Application.Operations.UserGroups.Dtos;

namespace Fargo.Application.Persistence.Repositories;

public interface IUserGroupQueryRepository
{
    Task<UserGroupDto?> GetInfoByGuidAsync(
        Guid userGroupGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserGroupDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
