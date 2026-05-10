using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoAuthenticationGrpcClient(
    Contracts.AuthenticationGrpc.AuthenticationGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.AuthInfo> LoginAsync(
        Contracts.LoginRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.LoginAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> LogoutAsync(
        Contracts.RefreshRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.LogoutAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.AuthInfo> RefreshAsync(
        Contracts.RefreshRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.RefreshAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> ChangePasswordAsync(
        Contracts.PasswordUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.ChangePasswordAsync(request, headers, deadline, ct),
            cancellationToken);
}
