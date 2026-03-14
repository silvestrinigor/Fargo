using Fargo.Application.Enums;

namespace Fargo.Application.Models.TreeModels;

public sealed record TreeNode(
    string NodeId,
    Guid EntityGuid,
    TreeNodeType NodeType,
    string Title,
    string? Subtitle,
    string? ParentNodeId,
    bool HasChildren,
    int? ChildCount = null,
    Guid? PartitionGuid = null,
    string? PartitionName = null,
    bool IsActive = true,
    IReadOnlyCollection<TreeBadge>? Badges = null,
    IReadOnlyDictionary<string, string?>? Metadata = null
);