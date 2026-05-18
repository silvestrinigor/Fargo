namespace Fargo.Sdk;

public sealed record FargoWorkspace(
    string CorrelationId,
    Guid WorkspaceGuid);

public sealed record FargoQueuedCommand(
    string CorrelationId,
    Guid WorkspaceGuid,
    string CommandId,
    int Sequence,
    Guid? ReservedEntityGuid);

public sealed record FargoWorkspaceCommit(
    string CorrelationId,
    Guid WorkspaceGuid,
    IReadOnlyCollection<FargoCommittedCommand> Commands);

public sealed record FargoCommittedCommand(
    string CommandId,
    int Sequence,
    Guid? EntityGuid);
