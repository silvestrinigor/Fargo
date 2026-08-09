using Fargo.Application;
using Fargo.Application.Shared.UserGroups;

namespace Fargo.HttpApi.Client.UserGroups;

public interface IUserGroupHttpApiClient
{
    Task<UserGroupDto?> GetAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserGroupDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(UserGroupCreateDto request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid userGroupGuid, UserGroupUpdateDto request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default);
}
