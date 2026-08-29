using Fargo.Http.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Fargo.Http.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoHttpClient(
        IServiceCollection services,
        Uri baseAddress,
        Func<CancellationToken, Task<string>> accessTokenProvider)
    {
        services.AddSingleton<IAuthenticationProvider>(
            new FargoAuthenticationProvider(accessTokenProvider));

        services.AddHttpClient<FargoApiClient>();

        services.AddSingleton(sp =>
        {
            var authenticationProvider =
                sp.GetRequiredService<IAuthenticationProvider>();

            var requestAdapter =
                new HttpClientRequestAdapter(
                    authenticationProvider)
                {
                    BaseUrl = baseAddress.ToString()
                };

            return new FargoApiClient(requestAdapter);
        });

        return services;
    }
}
