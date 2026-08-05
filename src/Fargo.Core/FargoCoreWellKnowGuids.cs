namespace Fargo.Core;

/// <summary>
/// Provides well-known system GUIDs used by Fargo.
/// </summary>
public static class FargoCoreWellKnowGuids
{
    /// <summary>
    /// The string representation of the global partition identifier.
    /// </summary>
    public const string GlobalPartitionGuidString = "00000000-0000-0000-0000-000000000002";

    /// <summary>
    /// Gets the global partition identifier.
    /// </summary>
    public static Guid GlobalPartitionGuid => new(GlobalPartitionGuidString);

    /// <summary>
    /// The string representation of the administrator user group identifier.
    /// </summary>
    public const string AdminUserGroupGuidString = "00000000-0000-0000-0000-000000000003";

    /// <summary>
    /// Gets the administrator user group identifier.
    /// </summary>
    public static Guid AdminUserGroupGuid => new(AdminUserGroupGuidString);

    /// <summary>
    /// The string representation of the administrator user identifier.
    /// </summary>
    public const string AdminUserGuidString = "00000000-0000-0000-0000-000000000004";

    /// <summary>
    /// Gets the administrator user identifier.
    /// </summary>
    public static Guid AdminUserGuid => new(AdminUserGuidString);
}
