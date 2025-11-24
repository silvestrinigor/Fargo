using Fargo.Application.Interfaces.Http;
using Fargo.Application.Services;
using Fargo.Application.Services.Interfaces;
using Fargo.Infrastructure.Http.Fargo;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

public static class ExternalServiceCollectionExtensions
{
    public static IServiceCollection AddExternalInfrastructure(this IServiceCollection services)
    {

        services.AddScoped<IArticleApplicationService, ArticleApplicationHttpClientService>();

        services.AddHttpClient<IArticleHttpClientService, ArticleHttpClientService>(client =>
        {
            client.BaseAddress = new("https+http://apiservice");
        });

        return services;
    }
}
