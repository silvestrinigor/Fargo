using Fargo.Client.Authentication;
using Fargo.Client.ErrorHandling;
using Fargo.Client.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoHttpClient(this IServiceCollection services, Uri baseAddress)
    {
        services.AddKiotaHandlers();

        services.AddSingleton<ClientConnectionContext>();

        // Token store, refresher and access-token source. The refresher follows
        // the same server override as the API client.
        services.AddFargoClientAuthentication(sp =>
            sp.GetRequiredService<ClientConnectionContext>().BaseAddressOverride ?? baseAddress);

        services.AddTransient<ProblemDetailsHandler>();

        services.AddSingleton<IAccessTokenProvider, FargoAccessTokenProvider>();

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
        // sees the final response, letting it translate 400/401/403 into typed
        // exceptions before Kiota raises its generic ApiException.
        .AddHttpMessageHandler<ProblemDetailsHandler>()
        .AttachKiotaHandlers();

        services.AddTransient(sp => sp.GetRequiredService<FargoClientFactory>().GetClient());

        return services;
    }
}
