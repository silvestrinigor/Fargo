using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents a user group returned by the API.</summary>
public sealed record UserGroupInfo(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionInfo> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid = null);
