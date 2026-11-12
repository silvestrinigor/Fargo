using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.ErrorHandling;
using Fargo.ClientHttp.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.ClientHttp.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoHttpClient(this IServiceCollection services, Uri baseAddress)
    {
        services.AddKiotaHandlers();

        services.AddSingleton<ClientConnectionContext>();

        services.AddTransient<ProblemDetailsHandler>();

        services.AddSingleton<IAccessTokenProvider, FargoAccessTokenProvider>();

        services.AddSingleton<ITokenStore, TokenStore>();

        services.AddSingleton<IAuthenticationProvider>(sp =>
        {
            var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();

            return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
        });

        services.AddHttpClient<FargoClientFactory>((sp, client) =>
        {
            var connectionContext = sp.GetRequiredService<ClientConnectionContext>();

            client.BaseAddress = connectionContext.BaseAddressOverride ?? baseAddress;
        })
        // Registered before the Kiota handlers so it is the outermost handler and
        // sees the final response, letting it translate 401/403 into typed
        // exceptions before Kiota raises its generic ApiException.
        .AddHttpMessageHandler<ProblemDetailsHandler>()
        .AttachKiotaHandlers();

        services.AddTransient(sp => sp.GetRequiredService<FargoClientFactory>().GetClient());

        return services;
    }
}
