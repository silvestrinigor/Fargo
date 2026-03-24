namespace Fargo.Application.Models.TreeModels;

/// <summary>
/// Defines the types of nodes that can exist in a tree structure.
/// </summary>
/// <remarks>
/// The numeric values of this enum are persisted as part of <see cref="Nodeid"/>.
/// Therefore, values must remain stable over time and should never be reordered
/// or changed once in use.
/// </remarks>
public enum TreeNodeType
{
    /// <summary>
    /// Represents a partition node.
    /// </summary>
    Partition = 1,

    /// <summary>
    /// Represents a user group node.
    /// </summary>
    UserGroup = 2,

    /// <summary>
    /// Represents a user node.
    /// </summary>
    User = 3,

    /// <summary>
    /// Represents an article node.
    /// </summary>
    Article = 4,

    /// <summary>
    /// Represents an item node.
    /// </summary>
    Item = 5
}
