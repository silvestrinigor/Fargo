namespace Fargo.Sdk.Models;

public sealed class UserInfo
{
    public Guid Guid { get; set; }
    public string Nameid { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Description { get; set; } = string.Empty;
    public TimeSpan DefaultPasswordExpirationPeriod { get; set; }
    public DateTimeOffset RequirePasswordChangeAt { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<PermissionInfo> Permissions { get; set; } = [];
    public IReadOnlyCollection<Guid> PartitionAccesses { get; set; } = [];
}
