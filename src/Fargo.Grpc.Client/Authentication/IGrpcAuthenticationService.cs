namespace Fargo.Grpc.Client.Authentication;

public interface IGrpcAuthenticationService
{
    Task LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);

    Task RefreshAsync(
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        CancellationToken cancellationToken = default);
}
