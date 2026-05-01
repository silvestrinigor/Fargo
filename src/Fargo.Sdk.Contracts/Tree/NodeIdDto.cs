namespace Fargo.Sdk.Contracts.Tree;

/// <summary>Represents a node identifier in an entity tree.</summary>
public sealed record NodeIdDto(TreeNodeType TreeNodeType, Guid EntityGuid);
