using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Persistence;
using Fargo.Application.Services;
using Fargo.Core.Contracts;
using Fargo.Core.Services;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<FargoContext>(opt =>
            opt.UseInMemoryDatabase("Fargo"));

        services.AddScoped<IEntityMainRepository, EntityRepository>();

        services.AddScoped<AreaService>();
        services.AddScoped<IAreaApplicationService, AreaApplicationService>();
        services.AddScoped<IAreaRepository, AreaRepository>();

        services.AddScoped<IArticleApplicationService, ArticleApplicationService>();
        services.AddScoped<IArticleRepository, ArticleRepository>();

        services.AddScoped<ContainerService>();
        services.AddScoped<IContainerRepository, ContainerRepository>();
        services.AddScoped<IContainerApplicationService, ContainerApplicationService>();

        services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

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
