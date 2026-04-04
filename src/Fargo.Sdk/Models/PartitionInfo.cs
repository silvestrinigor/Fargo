namespace Fargo.Sdk.Models;

public sealed class PartitionInfo
{
    public Guid Guid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentPartitionGuid { get; set; }
    public bool IsActive { get; set; }
}
