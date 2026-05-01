namespace Fargo.Sdk.Contracts.Tree;

/// <summary>Represents an entity-tree node returned by the API.</summary>
public sealed record EntityTreeNodeDto(
    NodeIdDto Nodeid,
    string Title,
    string? Subtitle,
    bool HasChildren,
    bool IsActive)
{
    public TreeNodeType TreeNodeType => Nodeid.TreeNodeType;

    public Guid EntityGuid => Nodeid.EntityGuid;
}
