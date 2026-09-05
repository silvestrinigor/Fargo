using Fargo.Http.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace Fargo.Http.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoHttpClient(
        this IServiceCollection services,
        Uri baseAddress)
    {
        services.AddHttpClient();

        services.AddSingleton<IAccessTokenProvider, FargoAccessTokenProvider>();

        services.AddSingleton<IAuthenticationProvider>(sp =>
        {
            var accessTokenProvider =
                sp.GetRequiredService<IAccessTokenProvider>();

            return new BaseBearerTokenAuthenticationProvider(
                accessTokenProvider);
        });

        services.AddSingleton(sp =>
        {
            var authenticationProvider =
                sp.GetRequiredService<IAuthenticationProvider>();

            var requestAdapter =
                new HttpClientRequestAdapter(authenticationProvider)
                {
                    BaseUrl = baseAddress.ToString()
                };

            return new FargoApiClient(requestAdapter);
        });

        return services;
    }
}
