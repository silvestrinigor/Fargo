using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Repositories;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Infrastructure.Persistence.Read.Repositories;
using Fargo.Infrastructure.Persistence.Write;
using Fargo.Infrastructure.Persistence.Write.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddInfrastructure()
            {
                services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

                services.AddScoped<ICommandHandler<ArticleCreateCommand, Task<Guid>>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleDeleteCommand, Task>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleUpdateCommand, Task>, ArticleUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<ArticleSingleQuery, Task<ArticleReadModel?>>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ArticleManyQuery, Task<IEnumerable<ArticleReadModel>>>, ArticleManyQueryHandler>();

                services.AddScoped<ICommandHandler<ItemCreateCommand, Task<Guid>>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ItemDeleteCommand, Task>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ItemUpdateCommand, Task>, ItemUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<ItemSingleQuery, Task<ItemReadModel?>>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, Task<IEnumerable<ItemReadModel>>>, ItemManyQueryHandler>();

                services.AddScoped<ICommandHandler<UserCreateCommand, Task<Guid>>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserDeleteCommand, Task>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserUpdateCommand, Task>, UserUpdateCommandHandler>();
                services.AddScoped<ICommandHandler<UserPermissionUpdateCommand, Task>, UserPermissionUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<UserSingleQuery, Task<UserReadModel?>>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, Task<IEnumerable<UserReadModel>>>, UserManyQueryHandler>();
                services.AddScoped<IQueryHandler<UserPermissionManyQuery, Task<IEnumerable<PermissionReadModel>?>>, UserPermissionAllQueryHandler>();

                services.AddScoped<ICommandHandler<PartitionCreateCommand, Task<Guid>>, PartitionCreateCommandHandler>();
                services.AddScoped<ICommandHandler<PartitionDeleteCommand, Task>, PartitionDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<PartitionUpdateCommand, Task>, PartitionUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<PartitionSingleQuery, Task<PartitionReadModel?>>, PartitionSingleQueryHandler>();
                services.AddScoped<IQueryHandler<PartitionManyQuery, Task<IEnumerable<PartitionReadModel>>>, PartitionManyQueryHandler>();

                services.AddScoped<ArticleService>();

                services.AddScoped<ItemService>();

                services.AddScoped<UserService>();

                services.AddScoped<PartitionService>();

                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IArticleReadRepository, ArticleReadRepository>();

                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IItemReadRepository, ItemReadRepository>();

                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserReadRepository, UserReadRepository>();

                services.AddScoped<IPartitionReadRepository, PartitionReadRepository>();
                services.AddScoped<IPartitionRepository, PartitionRepository>();

                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

                return services;
            }
        }

        extension(IServiceProvider services)
        {
            public async Task<IServiceProvider> InitInfrastructureAsync()
            {
                using (var scope = services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FargoWriteDbContext>();
                    dbContext.Database.EnsureCreated();
                }

                return services;
            }
        }
    }
}