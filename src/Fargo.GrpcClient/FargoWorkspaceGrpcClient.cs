using Fargo.GrpcContracts.Commands.V1;
using Grpc.Core;

namespace Fargo.GrpcClient;

public sealed class FargoWorkspaceGrpcClient(
    WorkspaceService.WorkspaceServiceClient client,
    FargoGrpcClientOptions options
) : IFargoWorkspaceClient
{
    public async Task<BeginWorkspaceResponse> BeginWorkspaceAsync(
        BeginWorkspaceRequest request,
        CancellationToken cancellationToken = default)
        => await client.BeginWorkspaceAsync(
            request,
            await CreateMetadata(cancellationToken),
            CreateDeadline(),
            cancellationToken);

    public async Task<QueueCommandResponse> QueueCommandAsync(
        QueueCommandRequest request,
        CancellationToken cancellationToken = default)
        => await client.QueueCommandAsync(
            request,
            await CreateMetadata(cancellationToken),
            CreateDeadline(),
            cancellationToken);

    public async Task<CommitWorkspaceResponse> CommitWorkspaceAsync(
        CommitWorkspaceRequest request,
        CancellationToken cancellationToken = default)
        => await client.CommitWorkspaceAsync(
            request,
            await CreateMetadata(cancellationToken),
            CreateDeadline(),
            cancellationToken);

    public async Task RollbackWorkspaceAsync(
        RollbackWorkspaceRequest request,
        CancellationToken cancellationToken = default)
        => await client.RollbackWorkspaceAsync(
            request,
            await CreateMetadata(cancellationToken),
            CreateDeadline(),
            cancellationToken);

    private DateTime? CreateDeadline()
        => options.DefaultTimeout is null
            ? null
            : DateTime.UtcNow.Add(options.DefaultTimeout.Value);

    private async Task<Metadata> CreateMetadata(CancellationToken cancellationToken)
    {
        var headers = new Metadata();

        if (options.BearerTokenProvider is null)
        {
            return headers;
        }

        var token = await options.BearerTokenProvider(cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            headers.Add("authorization", $"Bearer {token}");
        }

        return headers;
    }
}
