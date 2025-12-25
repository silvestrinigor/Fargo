using Fargo.Application.Persistence;
using Fargo.Application.Services;
using Fargo.Domain.Interfaces.Repositories;
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

        services.AddScoped<IArticleService, ArticleService>();

        services.AddScoped<IItemService, ItemService>();

        services.AddScoped<IContainerService, ContainerService>();

        services.AddScoped<IEntityRepository, EntityRepository>();

        services.AddScoped<IArticleRepository, ArticleRepository>();

        services.AddScoped<IItemRepository, ItemRepository>();

        services.AddScoped<IContainerRepository, ContainerRepository>();

        services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

        return services;
    }

    public async static Task<IServiceProvider> InitInfrastructureAsync(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FargoContext>();
            dbContext.Database.EnsureCreated();
        }

        return services;
    }
}
