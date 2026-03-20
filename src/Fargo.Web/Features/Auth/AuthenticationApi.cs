using Fargo.Application.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.Infrastructure.Client.Clients.Serialization;
using Fargo.Web.Api;
using System.Net.Http.Json;

namespace Fargo.Web.Features.Auth;

public sealed class AuthenticationApi(IHttpClientFactory httpClientFactory)
{
    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    public async Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("FargoApi");
        var response = await client.PostAsJsonAsync("/authentication/login", command, FargoJsonSerializerOptions.Default, cancellationToken);

        await FargoApiErrors.EnsureSuccessAsync(response, cancellationToken);

        return (await response.Content.ReadFromJsonAsync<AuthResult>(FargoJsonSerializerOptions.Default, cancellationToken))
            ?? throw new InvalidOperationException("The authentication endpoint returned an empty response.");
    }
}
