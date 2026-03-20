namespace Fargo.Application.Models.TreeModels;

public sealed record TreeNode(
    Nodeid Nodeid,
    TreeNodeType TreeNodeType,
    Guid EntityGuid,
    string Title,
    string? Subtitle,
    Nodeid? ParentNodeId,
    int MembersCount,
    bool IsActive = true)
{
    public bool HasChildren => MembersCount > 0;
}
