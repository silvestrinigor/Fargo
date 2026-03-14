using Fargo.HttpApi.Client.Contracts;
using Fargo.Infrastructure.Client.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFargoHttpApiClient(
        this IServiceCollection services)
    {
        var baseUrl = "http://apiservice";

        services.AddHttpClient<IAuthenticationClient, AuthenticationClient>(c => c.BaseAddress = new Uri(baseUrl));
        services.AddHttpClient<IArticleClient, ArticleClient>(c => c.BaseAddress = new Uri(baseUrl));
        services.AddHttpClient<IItemClient, ItemClient>(c => c.BaseAddress = new Uri(baseUrl));
        services.AddHttpClient<IUserClient, UserClient>(c => c.BaseAddress = new Uri(baseUrl));
        services.AddHttpClient<IUserGroupClient, UserGroupClient>(c => c.BaseAddress = new Uri(baseUrl));

        return services;
    }
}