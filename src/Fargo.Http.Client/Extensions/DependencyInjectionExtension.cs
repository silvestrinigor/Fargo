using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Http.Client.Extensions;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddFargoHttpClient(this IServiceCollection services, Uri baseAddress)
    {
        services.AddKiotaHandlers();

        services.AddSingleton<IAccessTokenProvider, FargoAccessTokenProvider>();

        services.AddSingleton<ITokenStore, TokenStore>();

        services.AddSingleton<IAuthenticationProvider>(sp =>
        {
            var accessTokenProvider = sp.GetRequiredService<IAccessTokenProvider>();

            return new BaseBearerTokenAuthenticationProvider(accessTokenProvider);
        });

        services.AddHttpClient<FargoClientFactory>((sp, client) =>
        {
            client.BaseAddress = baseAddress;
        }).AttachKiotaHandlers();

        services.AddTransient(sp => sp.GetRequiredService<FargoClientFactory>().GetClient());

        return services;
    }
}
