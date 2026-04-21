using Fargo.Application;
using Fargo.Application.Tree;
using Fargo.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Extensions;

public static class TreeServiceCollectionExtensions
{
    public static IServiceCollection AddFargoTreeInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPartitionTreeRepository, PartitionTreeRepository>();
        services.AddScoped<IArticleTreeRepository, ArticleTreeRepository>();
        services.AddScoped<IUserTreeRepository, UserTreeRepository>();

        services.AddScoped<IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<EntityTreeNode>>, PartitionTreeQueryHandler>();
        services.AddScoped<IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<EntityTreeNode>>, ArticleTreeQueryHandler>();
        services.AddScoped<IQueryHandler<UserTreeQuery, IReadOnlyCollection<EntityTreeNode>>, UserTreeQueryHandler>();

        return services;
    }
}
