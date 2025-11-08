using Fargo.Application.Contracts;
using Fargo.Application.Contracts.ExternalServices;
using Fargo.Application.Contracts.UnitOfWork;
using Fargo.Application.Services;
using Fargo.Core.Factories;
using Fargo.Infrastructure.Contexts;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Fargo.Core.Contracts;
using Fargo.Infrastructure.ExternalServices.Fargo;

namespace Fargo.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FargoContext>(opt =>
            opt.UseInMemoryDatabase("Fargo"));

        services.AddScoped<IArticleApplicationService, ArticleApplicationService>();

        services.AddScoped<IArticleFactory, ArticleFactory>();

        services.AddScoped<IArticleRepository, ArticleRepository>();

        services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

        return services;
    }

    public static IServiceCollection AddHttpClientInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<IArticleApplicationService, ArticleApplicationHttpClientService>();

        services.AddHttpClient<IArticleHttpClientService, ArticleHttpClientService>(client =>
        {
            client.BaseAddress = new("https+http://apiservice");
        });

        return services;
    }

    public static IServiceProvider InitInfrastructure(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FargoContext>();
            dbContext.Database.EnsureCreated();
        }

        return services;
    }
}
 