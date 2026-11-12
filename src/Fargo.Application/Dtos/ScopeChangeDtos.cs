using Fargo.Core.Identity.Credentials;

namespace Fargo.Application.Dtos;

/// <summary>
/// Request body of the dedicated user-group membership endpoint. At least one group must be given,
/// and none may be in both lists.
/// </summary>
public sealed record UserGroupsChangeDto(
    IReadOnlyCollection<Guid>? UserGroupsToAdd = null,
    IReadOnlyCollection<Guid>? UserGroupsToRemove = null
);

/// <summary>
/// Request body of the dedicated partition endpoints. Membership is the partitions the target
/// belongs to; access is the partitions it may reach. At least one list must be given, and no
/// partition may be in both the add and the remove list of the same kind.
/// </summary>
public sealed record PartitionsChangeDto(
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToAdd = null,
    IReadOnlyCollection<Guid>? PartitionAccessesToRemove = null
);

/// <summary>
/// Request body of the dedicated partition endpoints of articles and items, which belong to partitions
/// but have no partition access of their own. At least one partition must be given, and none may be in
/// both lists.
/// </summary>
public sealed record PartitionMembershipsChangeDto(
    IReadOnlyCollection<Guid>? PartitionsToAdd = null,
    IReadOnlyCollection<Guid>? PartitionsToRemove = null
);

public sealed record ParentUserGroupDto(Guid ParentUserGroupGuid);

public sealed record ParentPartitionDto(Guid ParentPartitionGuid);

public sealed record PasswordResetDto(Password Password);

/// <summary>
/// How long a user's password stays valid by default. The period is not named after the password so
/// the audit log does not redact it.
/// </summary>
public sealed record PasswordExpirationDto(TimeSpan ExpirationPeriod);
