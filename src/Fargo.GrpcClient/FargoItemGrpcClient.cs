using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoItemGrpcClient(
    Contracts.ItemsGrpc.ItemsGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.ItemInfo> GetItemAsync(
        Contracts.GetItemRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetItemAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.ItemList> GetItemsAsync(
        Contracts.GetManyRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetItemsAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.GuidResult> CreateItemAsync(
        Contracts.ItemCreateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.CreateItemAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> UpdateItemAsync(
        Contracts.ItemUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.UpdateItemAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> DeleteItemAsync(
        Contracts.GuidRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.DeleteItemAsync(request, headers, deadline, ct),
            cancellationToken);
}
