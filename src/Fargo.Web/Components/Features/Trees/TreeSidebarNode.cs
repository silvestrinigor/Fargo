using Fargo.Sdk.Articles;
using Fargo.Sdk.Items;
using Fargo.Sdk.Partitions;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;

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

    public static TreeSidebarNode FromItem(Item item, string? articleTitle = null) => new()
    {
        EntityGuid = item.Guid,
        Title = articleTitle ?? item.Guid.ToString("N")[..8],
        IsActive = true,
        HasChildren = false
    };

    public static TreeSidebarNode FromUser(User user) => new()
    {
        EntityGuid = user.Guid,
        Title = user.Nameid,
        IsActive = user.IsActive,
        HasChildren = false
    };
}
