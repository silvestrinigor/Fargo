using Fargo.Application.Contracts;
using Fargo.Application.Contracts.Http;
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

        services.AddScoped<PartitionService>();
        services.AddScoped<IPartitionApplicationService, PartitionApplicationService>();
        services.AddScoped<IPartitionRepository, PartitionRepository>();

        services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

        services.AddScoped<IRootEntitiesService, RootEntitiesService>();

        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();

        return services;
    }

    public async static Task<IServiceProvider> InitInfrastructureAsync(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FargoContext>();
            dbContext.Database.EnsureCreated();

            var service = scope.ServiceProvider.GetRequiredService<IRootEntitiesService>();
            await service.EnsureRootAreaExistAsync();
        }

        return services;
    }
}
