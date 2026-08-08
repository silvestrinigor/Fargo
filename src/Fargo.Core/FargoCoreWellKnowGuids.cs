namespace Fargo.Core;

/// <summary>
/// Provides well-known system GUIDs used by Fargo.
/// </summary>
public static class FargoCoreWellKnowGuids
{
    /// <summary>
    /// The string representation of the global partition identifier.
    /// </summary>
    private const string globalPartitionGuidString = "00000000-0000-0000-0000-000000000002";

    /// <summary>
    /// The string representation of the administrator user group identifier.
    /// </summary>
    private const string administratorsUserGroupGuidString = "00000000-0000-0000-0000-000000000003";

    /// <summary>
    /// The string representation of the administrator user identifier.
    /// </summary>
    private const string adminUserGuidString = "00000000-0000-0000-0000-000000000004";

    /// <summary>
    /// Gets the global partition identifier.
    /// </summary>
    public static Guid GlobalPartitionGuid => new(globalPartitionGuidString);

    /// <summary>
    /// Gets the administrator user group identifier.
    /// </summary>
    public static Guid AdministratorsUserGroupGuid => new(administratorsUserGroupGuidString);

    /// <summary>
    /// Gets the administrator user identifier.
    /// </summary>
    public static Guid AdminUserGuid => new(adminUserGuidString);
}
