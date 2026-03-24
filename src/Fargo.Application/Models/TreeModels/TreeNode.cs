namespace Fargo.Application.Models.TreeModels;

public sealed record TreeNode(
    Nodeid Nodeid,
    string Title,
    string? Subtitle,
    Nodeid? ParentNodeId,
    int MembersCount,
    bool IsActive = true)
{
    public bool HasChildren => MembersCount > 0;

    public TreeNodeType TreeNodeType => Nodeid.TreeNodeType;

    public Guid EntityGuid => Nodeid.EntityGuid;
}
