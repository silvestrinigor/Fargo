using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFargoHttpApiClient(
        this IServiceCollection services,
        string baseUrl = "http://apiservice",
        Action<IHttpClientBuilder>? configureClient = null)
    {
        AddClient<IArticleClient, ArticleClient>(services, baseUrl, configureClient);
        AddClient<IAuthenticationClient, AuthenticationClient>(services, baseUrl, configureClient);
        AddClient<IItemClient, ItemClient>(services, baseUrl, configureClient);
        AddClient<IPartitionClient, PartitionClient>(services, baseUrl, configureClient);
        AddClient<IUserClient, UserClient>(services, baseUrl, configureClient);
        AddClient<IUserGroupClient, UserGroupClient>(services, baseUrl, configureClient);

        return services;
    }

    private static void AddClient<TContract, TImplementation>(
        IServiceCollection services,
        string baseUrl,
        Action<IHttpClientBuilder>? configureClient)
        where TContract : class
        where TImplementation : class, TContract
    {
        var builder = services.AddHttpClient<TContract, TImplementation>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        configureClient?.Invoke(builder);
    }
}
