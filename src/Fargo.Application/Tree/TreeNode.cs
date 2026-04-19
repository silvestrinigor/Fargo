namespace Fargo.Application.Tree;

/// <summary>
/// Represents a node in a hierarchical tree structure.
/// </summary>
/// <remarks>
/// This model is typically used as a lightweight projection for tree-based
/// representations (e.g., UI components), avoiding the need to load full
/// domain entities.
///
/// The node identity is encapsulated in <see cref="Nodeid"/>, from which
/// the <see cref="TreeNodeType"/> and <see cref="EntityGuid"/> are derived.
/// </remarks>
/// <param name="Nodeid">
/// The strongly-typed identifier of the node, containing both its type
/// and associated entity identifier.
/// </param>
/// <param name="Title">
/// The display title of the node.
/// </param>
/// <param name="Subtitle">
/// An optional secondary text providing additional context.
/// </param>
/// <param name="HasChildren">
/// Indicates whether the node contains related members or children.
/// </param>
/// <param name="IsActive">
/// Indicates whether the node is active and should be considered in use.
/// </param>
public sealed record EntityTreeNode(
    Nodeid Nodeid,
    string Title,
    string? Subtitle,
    bool HasChildren,
    bool IsActive)
{
    /// <summary>
    /// Gets the type of the node.
    /// </summary>
    public TreeNodeType TreeNodeType => Nodeid.TreeNodeType;

    /// <summary>
    /// Gets the unique identifier of the underlying entity.
    /// </summary>
    public Guid EntityGuid => Nodeid.EntityGuid;
}
