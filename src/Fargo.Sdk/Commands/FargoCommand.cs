namespace Fargo.Sdk.Commands;

public abstract record FargoCommand;

public sealed record ArticleCreateCommand(string Name) : FargoCommand;

public sealed record ArticleRenameCommand(Guid ArticleGuid, string Name) : FargoCommand;

public sealed record ItemCreateCommand(Guid ArticleGuid, DateTimeOffset? ProductionDate = null) : FargoCommand;

public sealed record ItemMoveToContainerCommand(Guid ItemGuid, Guid? ParentContainerGuid = null) : FargoCommand;

public sealed record PartitionCreateCommand(string Name) : FargoCommand;

public sealed record PartitionRenameCommand(Guid PartitionGuid, string Name) : FargoCommand;

public sealed record PartitionMoveCommand(Guid PartitionGuid, Guid ParentPartitionGuid) : FargoCommand;
