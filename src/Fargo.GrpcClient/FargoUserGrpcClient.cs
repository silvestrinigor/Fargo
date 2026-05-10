using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoUserGrpcClient(
    Contracts.UsersGrpc.UsersGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.UserInfo> GetUserAsync(
        Contracts.GetUserRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetUserAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.UserList> GetUsersAsync(
        Contracts.GetManyRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetUsersAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.GuidResult> CreateUserAsync(
        Contracts.UserCreateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.CreateUserAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> UpdateUserAsync(
        Contracts.UserUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.UpdateUserAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> DeleteUserAsync(
        Contracts.GuidRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.DeleteUserAsync(request, headers, deadline, ct),
            cancellationToken);
}
