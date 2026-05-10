using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoUserGroupGrpcClient(
    Contracts.UserGroupsGrpc.UserGroupsGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.UserGroupInfo> GetUserGroupAsync(
        Contracts.GetUserGroupRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetUserGroupAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.UserGroupList> GetUserGroupsAsync(
        Contracts.GetManyRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetUserGroupsAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.GuidResult> CreateUserGroupAsync(
        Contracts.UserGroupCreateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.CreateUserGroupAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> UpdateUserGroupAsync(
        Contracts.UserGroupUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.UpdateUserGroupAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> DeleteUserGroupAsync(
        Contracts.GuidRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.DeleteUserGroupAsync(request, headers, deadline, ct),
            cancellationToken);
}
