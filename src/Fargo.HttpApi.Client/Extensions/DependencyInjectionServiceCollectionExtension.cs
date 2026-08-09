using Fargo.HttpApi.Client.Articles;
using Fargo.HttpApi.Client.Common;
using Fargo.HttpApi.Client.Identity;
using Fargo.HttpApi.Shared.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Fargo.HttpApi.Client.Extensions;

public static class DependencyInjectionServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoHttpApiClient(Uri baseAddress)
        {
            var jsonSerializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            jsonSerializeOptions.AddFargoJsonConverters();

            services.AddSingleton(jsonSerializeOptions);

            services.AddHttpClient<FargoHttpClient>(client =>
            {
                client.BaseAddress = baseAddress;
            });

            services.AddScoped<IIdentityHttpApiClient, IdentityHttpApiClient>();

            services.AddScoped<IArticleHttpApiClient, ArticleHttpApiClient>();

            return services;
        }
    }
}
