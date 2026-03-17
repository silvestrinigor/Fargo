using Fargo.Application.Models.UserGroupModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface IUserGroupClient
{
    Task<UserGroupInformation?> GetSingleAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserGroupInformation>> GetManyAsync(
        Guid? userGuid,
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        UserGroupCreateModel model,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userGroupGuid,
        UserGroupUpdateModel model,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}
