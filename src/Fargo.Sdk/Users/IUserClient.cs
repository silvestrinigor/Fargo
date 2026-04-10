namespace Fargo.Sdk.Users;

public interface IUserClient
{
    Task<FargoSdkResponse<UserResult>> GetAsync(
        Guid userGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<UserResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(
        string nameid,
        string password,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid userGuid,
        string? nameid = null,
        string? firstName = null,
        string? lastName = null,
        string? description = null,
        string? password = null,
        bool? isActive = null,
        IReadOnlyCollection<ActionType>? permissions = null,
        TimeSpan? defaultPasswordExpirationPeriod = null,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid userGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(
        Guid userGuid,
        Guid userGroupGuid,
        CancellationToken cancellationToken = default);
}
