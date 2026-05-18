using Fargo.GrpcContracts.Commands.V1;

namespace Fargo.GrpcClient;

public interface IFargoWorkspaceClient
{
    Task<BeginWorkspaceResponse> BeginWorkspaceAsync(
        BeginWorkspaceRequest request,
        CancellationToken cancellationToken = default);

    Task<QueueCommandResponse> QueueCommandAsync(
        QueueCommandRequest request,
        CancellationToken cancellationToken = default);

    Task<CommitWorkspaceResponse> CommitWorkspaceAsync(
        CommitWorkspaceRequest request,
        CancellationToken cancellationToken = default);

    Task RollbackWorkspaceAsync(
        RollbackWorkspaceRequest request,
        CancellationToken cancellationToken = default);
}
