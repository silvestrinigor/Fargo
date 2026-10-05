using Fargo.Cli.Configurations;
using Fargo.Http.Client.Authentication;
using ktsu.CredentialCache;
using System.Text.Json;

namespace Fargo.Cli.Authentication;

public class FargoCliTokenStore : ITokenStore
{
    private readonly PersonaGUID personaGuid;

    private readonly CredentialCache credentials = CredentialCache.Instance;

    public FargoCliTokenStore(IFargoCliConfigurationStore configurationStore)
    {
        var config = configurationStore.Load();

        if (config.CredentialId == null)
        {
            config.CredentialId = CredentialCache.CreatePersonaGUID();

            configurationStore.Save(config);

            Console.WriteLine(config.CredentialId);
        }

        personaGuid = PersonaGUID.Create(config.CredentialId);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        credentials.Remove(personaGuid);

        return Task.CompletedTask;
    }

    public Task<AuthTokens?> GetAsync(CancellationToken cancellationToken = default)
    {
        if (!credentials.TryGet(personaGuid, out Credential? credential))
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        if (credential is not CredentialWithToken tokenCredential)
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        var data = JsonSerializer.Deserialize<AuthTokens>(
            tokenCredential.Token);

        if (data is null)
        {
            return Task.FromResult<AuthTokens?>(null);
        }

        return Task.FromResult<AuthTokens?>(data);
    }

    public Task SetAsync(AuthTokens tokens, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(tokens);

        credentials.AddOrReplace(personaGuid, new CredentialWithToken
        {
            Token = CredentialToken.Create(json)
        });

        return Task.CompletedTask;
    }
}
