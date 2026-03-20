namespace Fargo.Web.Features.Partitions;

public sealed class PartitionTreeNode
{
    public required Guid Guid { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid? ParentPartitionGuid { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsExpanded { get; set; }
    public bool IsLoadingChildren { get; set; }
    public bool ChildrenLoaded { get; set; }
    public List<PartitionTreeNode> Children { get; } = [];

    public static PartitionTreeNode FromSummary(PartitionSummary summary)
    {
        return new PartitionTreeNode
        {
            Guid = summary.Guid,
            Name = summary.Name,
            Description = summary.Description,
            ParentPartitionGuid = summary.ParentPartitionGuid,
            IsActive = summary.IsActive
        };
    }
}
