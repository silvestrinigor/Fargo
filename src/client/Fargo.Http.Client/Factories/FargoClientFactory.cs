using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Fargo.Http.Client.Factories;

public class FargoClientFactory
{
    private readonly IAuthenticationProvider _authenticationProvider;
    private readonly HttpClient _httpClient;

    public FargoClientFactory(HttpClient httpClient, IAuthenticationProvider authenticationProvider)
    {
        _authenticationProvider = authenticationProvider;
        _httpClient = httpClient;
    }

    public FargoApiClient GetClient()
    {
        return new FargoApiClient(new HttpClientRequestAdapter(_authenticationProvider, httpClient: _httpClient));
    }
}
