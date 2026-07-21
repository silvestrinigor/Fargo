namespace Fargo.Core;

/// <summary>
/// 
/// </summary>
public static class FargoDefaultGuids
{
    public const string SystemGuidString = "00000000-0000-0000-0000-000000000001";

    /// <summary>
    /// 
    /// </summary>
    public static Guid SystemGuid => new(SystemGuidString);

    public const string GlobalPartitionGuidString = "00000000-0000-0000-0000-000000000002";

    /// <summary>
    /// 
    /// </summary>
    public static Guid GlobalPartitionGuid => new(GlobalPartitionGuidString);

    public const string AdminUserGroupGuidString = "00000000-0000-0000-0000-000000000003";

    /// <summary>
    /// 
    /// </summary>
    public static Guid AdminUserGroupGuid => new(AdminUserGroupGuidString);

    public const string AdminUserGuidString = "00000000-0000-0000-0000-000000000004";

    /// <summary>
    /// 
    /// </summary>
    public static Guid AdminUserGuid => new(AdminUserGuidString);
}
