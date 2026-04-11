using Fargo.Sdk.Articles;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;

namespace Fargo.Web.Components.Features.Trees;

public sealed class TreeSidebarNode
{
    public required Guid EntityGuid { get; init; }

    public required string Title { get; init; }

    public string? Subtitle { get; init; }

    public bool IsActive { get; init; }

    public bool HasChildren { get; set; }

    public bool IsExpanded { get; set; }

    public bool IsLoadingChildren { get; set; }

    public bool ChildrenLoaded { get; set; }

    public List<TreeSidebarNode> Children { get; } = [];

    public static TreeSidebarNode FromPartition(Partition p) => new()
    {
        EntityGuid = p.Guid,
        Title = p.Name,
        IsActive = p.IsActive,
        HasChildren = true
    };

    public static TreeSidebarNode FromUserGroup(UserGroup ug) => new()
    {
        EntityGuid = ug.Guid,
        Title = ug.Nameid,
        IsActive = ug.IsActive,
        HasChildren = false
    };

    public static TreeSidebarNode FromArticle(Article a) => new()
    {
        EntityGuid = a.Guid,
        Title = a.Name,
        IsActive = true,
        HasChildren = false
    };
}
