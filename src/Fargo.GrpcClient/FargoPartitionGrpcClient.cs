using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoPartitionGrpcClient(
    Contracts.PartitionsGrpc.PartitionsGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.PartitionInfo> GetPartitionAsync(
        Contracts.GetPartitionRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetPartitionAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.PartitionList> GetPartitionsAsync(
        Contracts.GetManyRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetPartitionsAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.GuidResult> CreatePartitionAsync(
        Contracts.PartitionCreateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.CreatePartitionAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> UpdatePartitionAsync(
        Contracts.PartitionUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.UpdatePartitionAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> DeletePartitionAsync(
        Contracts.GuidRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.DeletePartitionAsync(request, headers, deadline, ct),
            cancellationToken);
}
