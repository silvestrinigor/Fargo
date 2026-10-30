using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Extensions;
using Fargo.ClientHttp.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.SimulationWorker;

/// <summary>
/// Builds independent, authenticated <see cref="FargoApiClient"/> instances for
/// a given user. Each client gets its own service provider and therefore its own
/// singleton <see cref="ITokenStore"/>, so sessions for different operators never
/// share a token and each operator gets its own per-user rate-limit bucket.
/// </summary>
internal sealed class FargoClientBuilder(Uri server)
{
    private readonly Uri _server = server;

    /// <summary>
    /// Logs in as the given user and returns a client whose requests carry that
    /// user's access token. Throws when authentication does not yield a complete
    /// token set.
    /// </summary>
    public async Task<FargoApiClient> CreateAuthenticatedClientAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var services = new ServiceCollection();
        services.AddFargoHttpClient(_server);
        var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<FargoApiClient>();
        var tokenStore = provider.GetRequiredService<ITokenStore>();

        var auth = await client.V1.Identity.Login.PostAsync(
            new IdentityLoginDto { Nameid = username, Password = password },
            cancellationToken: cancellationToken);

        if (auth?.AccessToken is null || auth.RefreshToken is null || auth.ExpiresAt is null)
        {
            throw new InvalidOperationException(
                $"Login for '{username}' did not return a complete token set.");
        }

        await tokenStore.SetAsync(
            new AuthTokens(auth.AccessToken, auth.RefreshToken, auth.ExpiresAt.Value),
            cancellationToken);

        return client;
    }
}
