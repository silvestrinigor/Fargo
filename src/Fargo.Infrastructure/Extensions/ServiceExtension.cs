using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Persistence;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Queries;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Infrastructure.Extensions;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
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
                services.AddScoped<IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>>, ArticleAllQueryHandler>();

                services.AddScoped<ICommandHandlerAsync<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<ItemSingleQuery, ItemDto?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>>, ItemManyQueryHandler>();

                services.AddScoped<ICommandHandlerAsync<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandlerAsync<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<UserSingleQuery, UserDto?>, UserSingleQueryHandler>();
                services.AddScoped<ICommandHandlerAsync<UserSetPermissionCommand>, UserSetPermissionCommandHandler>();
                services.AddScoped<IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<UserPermissionDto>>, UserPermissionAllQueryHandler>();
                services.AddScoped<IQueryHandlerAsync<UserAllQuery, IEnumerable<UserDto>>, UserAllQueryHandler>();

                services.AddScoped<ArticleService>();

                services.AddScoped<ItemService>();

                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IArticleReadRepository, ArticleReadRepository>();

                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IItemReadRepository, ItemReadRepository>();

                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserReadRepository, UserReadRepository>();

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
                    var dbContext = scope.ServiceProvider.GetRequiredService<FargoContext>();
                    dbContext.Database.EnsureCreated();
                }

                return services;
            }
        }
    }
}
