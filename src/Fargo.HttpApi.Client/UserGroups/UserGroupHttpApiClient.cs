using Fargo.Application;
using Fargo.Application.Shared.UserGroups;

namespace Fargo.HttpApi.Client.UserGroups;

public sealed class UserGroupHttApiClient : IUserGroupHttpApiClient
{
    public Task<Guid> CreateAsync(UserGroupCreateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<UserGroupDto?> GetAsync(Guid userGroupGuid, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyCollection<UserGroupDto>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid userGroupGuid, UserGroupUpdateDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
