using Fargo.Application.Models.TreeModels;
using Fargo.Application.Queries;
using Fargo.Application.Queries.TreeQueries;
using Fargo.Application.Repositories;
using Fargo.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Extensions;

public static class TreeServiceCollectionExtensions
{
    public static IServiceCollection AddFargoTreeInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPartitionTreeRepository, PartitionTreeRepository>();
        services.AddScoped<IArticleTreeRepository, ArticleTreeRepository>();
        services.AddScoped<IUserGroupTreeRepository, UserGroupTreeRepository>();
        services.AddScoped<IPartitionSecurityTreeRepository, PartitionSecurityTreeRepository>();

        services.AddScoped<IQueryHandler<PartitionTreeQuery, IReadOnlyCollection<TreeNode>>, PartitionTreeQueryHandler>();
        services.AddScoped<IQueryHandler<ArticleTreeQuery, IReadOnlyCollection<TreeNode>>, ArticleTreeQueryHandler>();
        services.AddScoped<IQueryHandler<UserGroupTreeQuery, IReadOnlyCollection<TreeNode>>, UserGroupTreeQueryHandler>();
        services.AddScoped<IQueryHandler<PartitionSecurityTreeQuery, IReadOnlyCollection<TreeNode>>, PartitionSecurityTreeQueryHandler>();

        return services;
    }
}
