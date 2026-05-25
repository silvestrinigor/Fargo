using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.HttpClient;

public static class FargoHttpClientServiceCollectionExtensions
{
    public static IHttpClientBuilder AddFargoHttpClient(
        this IServiceCollection services,
        Action<FargoHttpClientOptions> configure)
    {
        services.Configure(configure);
        services.AddTransient<FargoAuthorizationHandler>();

        var builder = services
            .AddHttpClient<IFargoHttpClient, FargoHttpClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<FargoHttpClientOptions>>().Value;

                httpClient.BaseAddress = options.BaseAddress
                    ?? throw new InvalidOperationException($"{nameof(FargoHttpClientOptions.BaseAddress)} must be configured.");
            })
            .AddHttpMessageHandler<FargoAuthorizationHandler>();

        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().Identity);
        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().Articles);
        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().Items);
        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().Users);
        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().UserGroups);
        services.AddTransient(static serviceProvider => serviceProvider.GetRequiredService<IFargoHttpClient>().Partitions);

        return builder;
    }
}
