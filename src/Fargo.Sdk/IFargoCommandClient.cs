using Fargo.Sdk.Commands;

namespace Fargo.Sdk;

public interface IFargoCommandClient
{
    Task<FargoWorkspace> BeginWorkspaceAsync(
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task<FargoQueuedCommand> QueueCommandAsync(
        Guid workspaceGuid,
        FargoCommand command,
        string? commandId = null,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task<FargoWorkspaceCommit> CommitWorkspaceAsync(
        Guid workspaceGuid,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    Task RollbackWorkspaceAsync(
        Guid workspaceGuid,
        string? correlationId = null,
        CancellationToken cancellationToken = default);
}
