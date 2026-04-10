namespace Fargo.Sdk.UserGroups;

public interface IUserGroupClient
{
    Task<FargoSdkResponse<UserGroupResult>> GetAsync(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<UserGroupResult>>> GetManyAsync(
        Guid? userGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(
        string nameid,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid userGroupGuid,
        string? nameid = null,
        string? description = null,
        bool? isActive = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}
