using Fargo.HttpApi.Client.Articles;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.HttpApi.Client.Extensions;

public static class DependencyInjectionServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoHttpApiClient()
        {
            services.AddScoped<IArticleHttpApiClient, ArticleHttpApiClient>();

            return services;
        }
    }
}
