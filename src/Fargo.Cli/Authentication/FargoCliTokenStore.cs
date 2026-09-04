
using Fargo.Cli.Configurations;
using Fargo.Http.Client.Authentication;
using ktsu.CredentialCache;

namespace Fargo.Cli.Authentication;

public class FargoCliTokenStore(CredentialCache credentials, IFargoCliConfigurationStore configurationStore) : ITokenStore
{
    private readonly PersonaGUID personaGuid = PersonaGUID.Create(configurationStore.Load().CredentialId);

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        credentials.Remove(personaGuid);

        return Task.CompletedTask;
    }

    public async Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (credentials.TryGet(personaGuid, out var stored) && stored is FargoCredential fargoCredential)
        {
            return new AuthTokens(
                fargoCredential.AccessToken,
                fargoCredential.AccessToken,
                fargoCredential.AccessTokenExpiresAt
            );
        }

        return null;
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        credentials.AddOrReplace(personaGuid, new FargoCredential
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            AccessTokenExpiresAt = tokens.ExpiresAt,
        });

        return Task.CompletedTask;
    }
}
