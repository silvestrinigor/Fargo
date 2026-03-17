using Fargo.Application.Models.UserModels;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Client.Contracts;

public interface IUserClient
{
    Task<UserInformation?> GetSingleAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserInformation>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        Page? page = null,
        Limit? limit = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        UserCreateModel model,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Guid userGuid,
        UserUpdateModel model,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    Task AddUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    Task RemoveUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}
