using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Users;

/// <summary>Low-level HTTP transport for user endpoints.</summary>
public interface IUserHttpClient
{
    Task<FargoSdkResponse<UserResult>> GetAsync(Guid userGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<UserResult>>> GetManyAsync(DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, Guid? partitionGuid = null, string? search = null, bool? noPartition = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<Guid>> CreateAsync(string nameid, string password, string? firstName = null, string? lastName = null, string? description = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, Guid? firstPartition = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid userGuid, string? nameid = null, string? firstName = null, string? lastName = null, string? description = null, string? password = null, bool? isActive = null, IReadOnlyCollection<ActionType>? permissions = null, TimeSpan? defaultPasswordExpirationPeriod = null, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid userGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> AddUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemoveUserGroupAsync(Guid userGuid, Guid userGroupGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(Guid userGuid, Guid partitionGuid, CancellationToken cancellationToken = default);

    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(Guid userGuid, CancellationToken cancellationToken = default);
}
