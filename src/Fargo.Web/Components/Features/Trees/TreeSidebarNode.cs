using Fargo.Application.Models.TreeModels;

namespace Fargo.Web.Components.Features.Trees;

public sealed class TreeSidebarNode
{
    public required Guid EntityGuid { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public bool IsActive { get; init; }

    public bool HasChildren { get; init; }

    public bool IsExpanded { get; set; }

    public bool IsLoadingChildren { get; set; }

    public bool ChildrenLoaded { get; set; }

    public List<TreeSidebarNode> Children { get; } = [];

    public static TreeSidebarNode FromTreeNode(TreeNode node) => new()
    {
        EntityGuid = node.EntityGuid,
        Title = node.Title,
        Subtitle = node.Subtitle,
        IsActive = node.IsActive,
        HasChildren = node.HasChilds
    };
}
