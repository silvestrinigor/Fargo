namespace Fargo.Sdk.Models;

public sealed class UserGroupInfo
{
    public Guid Guid { get; set; }
    public string Nameid { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public IReadOnlyCollection<PermissionInfo> Permissions { get; set; } = [];
}
