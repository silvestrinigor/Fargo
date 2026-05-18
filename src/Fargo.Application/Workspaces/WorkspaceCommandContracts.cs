namespace Fargo.Application.Workspaces;

public static class WorkspaceCommandTypes
{
    public const string ArticleCreate = "articles.create";
    public const string ArticleRename = "articles.rename";
    public const string ItemCreate = "items.create";
    public const string ItemMoveToContainer = "items.move-to-container";
    public const string PartitionCreate = "partitions.create";
    public const string PartitionRename = "partitions.rename";
    public const string PartitionMove = "partitions.move";
}

public sealed record WorkspaceCommandDraft(
    string CommandId,
    string CommandType,
    int CommandVersion,
    string PayloadJson);

public sealed record WorkspaceQueueCommandResult(
    Guid WorkspaceGuid,
    string CommandId,
    int Sequence,
    Guid? ReservedEntityGuid);

public sealed record WorkspaceCommitResult(
    Guid WorkspaceGuid,
    IReadOnlyCollection<WorkspaceCommittedCommandResult> Commands);

public sealed record WorkspaceCommittedCommandResult(
    string CommandId,
    int Sequence,
    Guid? EntityGuid);

public sealed record ArticleCreateWorkspaceCommand(
    Guid ArticleGuid,
    string Name);

public sealed record ArticleRenameWorkspaceCommand(
    Guid ArticleGuid,
    string Name);

public sealed record ItemCreateWorkspaceCommand(
    Guid ItemGuid,
    Guid ArticleGuid,
    DateTimeOffset? ProductionDate);

public sealed record ItemMoveToContainerWorkspaceCommand(
    Guid ItemGuid,
    Guid? ParentContainerGuid);

public sealed record PartitionCreateWorkspaceCommand(
    Guid PartitionGuid,
    string Name);

public sealed record PartitionRenameWorkspaceCommand(
    Guid PartitionGuid,
    string Name);

public sealed record PartitionMoveWorkspaceCommand(
    Guid PartitionGuid,
    Guid ParentPartitionGuid);
