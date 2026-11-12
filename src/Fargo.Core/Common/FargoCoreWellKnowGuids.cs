namespace Fargo.Core.Common;

public static class FargoCoreWellKnowGuids
{
    private const string globalPartitionGuidString = "00000000-0000-0000-0000-000000000002";

    private const string administratorsUserGroupGuidString = "00000000-0000-0000-0000-000000000003";

    private const string adminUserGuidString = "00000000-0000-0000-0000-000000000004";

    public static Guid GlobalPartitionGuid => new(globalPartitionGuidString);

    public static Guid AdministratorsUserGroupGuid => new(administratorsUserGroupGuidString);

    public static Guid AdminUserGuid => new(adminUserGuidString);
}
