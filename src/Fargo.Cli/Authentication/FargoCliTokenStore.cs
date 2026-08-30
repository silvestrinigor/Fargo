
using Fargo.Cli.Configurations;
using Fargo.Http.Client.Authentication;
using ktsu.CredentialCache;

namespace Fargo.Cli.Authentication;

public class FargoCliTokenStore(CredentialCache credentials, IFargoCliConfigurationStore configurationStore) : ITokenStore
{
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        var config = configurationStore.Load();

        var personaGuid = config.CredentialId;

        if (personaGuid is null)
        {
            return null;
        }

        if (credentials.TryGet(PersonaGUID.Create(personaGuid), out Credential? stored) && stored is CredentialWithUsernamePassword creds)
        {
        }

        throw new NotImplementedException();
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
