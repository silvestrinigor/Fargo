namespace Fargo.Core.Workspaces;

public enum WorkspaceStatus
{
    Pending = 0,
    Committed = 1,
    RolledBack = 2
}

public enum WorkspaceCommandStatus
{
    Pending = 0,
    Executed = 1
}

public enum WorkspaceReservedEntityKind
{
    Article = 0,
    Item = 1,
    Partition = 2
}

public sealed class Workspace : Entity
{
    private readonly List<WorkspaceCommand> commands = [];

    private Workspace()
    {
    }

    private Workspace(Guid actorGuid)
    {
        ActorGuid = actorGuid;
        Status = WorkspaceStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid ActorGuid { get; private init; }

    public WorkspaceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? CommittedAt { get; private set; }

    public DateTimeOffset? RolledBackAt { get; private set; }

    public IReadOnlyCollection<WorkspaceCommand> Commands => commands;

    public static Workspace Begin(Guid actorGuid)
        => new(actorGuid);

    public WorkspaceCommand QueueCommand(
        string commandId,
        int sequence,
        string commandType,
        int commandVersion,
        string payloadJson,
        Guid? reservedEntityGuid,
        WorkspaceReservedEntityKind? reservedEntityKind)
    {
        ValidateIsPending();

        if (string.IsNullOrWhiteSpace(commandId))
        {
            throw new ArgumentException("Command id is required.", nameof(commandId));
        }

        if (string.IsNullOrWhiteSpace(commandType))
        {
            throw new ArgumentException("Command type is required.", nameof(commandType));
        }

        var command = WorkspaceCommand.Create(
            this,
            commandId,
            sequence,
            commandType,
            commandVersion,
            payloadJson,
            reservedEntityGuid,
            reservedEntityKind);

        commands.Add(command);

        return command;
    }

    public void MarkCommitted()
    {
        ValidateIsPending();
        Status = WorkspaceStatus.Committed;
        CommittedAt = DateTimeOffset.UtcNow;
    }

    public void MarkRolledBack()
    {
        ValidateIsPending();
        Status = WorkspaceStatus.RolledBack;
        RolledBackAt = DateTimeOffset.UtcNow;
    }

    public void ValidateIsPending()
    {
        if (Status != WorkspaceStatus.Pending)
        {
            throw new InvalidOperationException($"Workspace '{Guid}' is not pending.");
        }
    }
}

public sealed class WorkspaceCommand : Entity
{
    private WorkspaceCommand()
    {
    }

    private WorkspaceCommand(
        Workspace workspace,
        string commandId,
        int sequence,
        string commandType,
        int commandVersion,
        string payloadJson,
        Guid? reservedEntityGuid,
        WorkspaceReservedEntityKind? reservedEntityKind)
    {
        Workspace = workspace;
        WorkspaceGuid = workspace.Guid;
        CommandId = commandId;
        Sequence = sequence;
        CommandType = commandType;
        CommandVersion = commandVersion;
        PayloadJson = payloadJson;
        ReservedEntityGuid = reservedEntityGuid;
        ReservedEntityKind = reservedEntityKind;
        Status = WorkspaceCommandStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid WorkspaceGuid { get; private init; }

    public Workspace Workspace { get; private init; } = null!;

    public string CommandId { get; private init; } = string.Empty;

    public int Sequence { get; private init; }

    public string CommandType { get; private init; } = string.Empty;

    public int CommandVersion { get; private init; }

    public string PayloadJson { get; private init; } = string.Empty;

    public Guid? ReservedEntityGuid { get; private init; }

    public WorkspaceReservedEntityKind? ReservedEntityKind { get; private init; }

    public WorkspaceCommandStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset? ExecutedAt { get; private set; }

    public static WorkspaceCommand Create(
        Workspace workspace,
        string commandId,
        int sequence,
        string commandType,
        int commandVersion,
        string payloadJson,
        Guid? reservedEntityGuid,
        WorkspaceReservedEntityKind? reservedEntityKind)
        => new(
            workspace,
            commandId,
            sequence,
            commandType,
            commandVersion,
            payloadJson,
            reservedEntityGuid,
            reservedEntityKind);

    public void MarkExecuted()
    {
        Status = WorkspaceCommandStatus.Executed;
        ExecutedAt = DateTimeOffset.UtcNow;
    }
}
