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
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence.Read.Repositories;
using Fargo.Infrastructure.Persistence.Write;
using Fargo.Infrastructure.Persistence.Write.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
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

                services.AddScoped<ICommandHandlerAsync<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<ArticleSingleQuery, ArticleReadModel?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<ArticleManyQuery, IEnumerable<ArticleReadModel>>, ArticleManyQueryHandler>();

                services.AddScoped<ICommandHandlerAsync<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<ItemUpdateCommand>, ItemUpdateCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<ItemSingleQuery, ItemReadModel?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemReadModel>>, ItemManyQueryHandler>();

                services.AddScoped<ICommandHandlerAsync<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<UserUpdateCommand>, UserUpdateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<UserPermissionUpdateCommand>, UserPermissionUpdateCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<UserSingleQuery, UserReadModel?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<UserManyQuery, IEnumerable<UserReadModel>>, UserManyQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<UserPermissionManyQuery, IEnumerable<PermissionReadModel>?>, UserPermissionAllQueryHandler>();

                services.AddScoped<ICommandHandlerAsync<PartitionCreateCommand, Guid>, PartitionCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<PartitionDeleteCommand>, PartitionDeleteCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<PartitionUpdateCommand>, PartitionUpdateCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<PartitionSingleQuery, PartitionReadModel?>, PartitionSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionReadModel>>, PartitionManyQueryHandler>();

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
