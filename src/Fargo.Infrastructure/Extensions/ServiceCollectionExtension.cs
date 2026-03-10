using Fargo.Application.Models;
using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Repositories;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Application.Security;
using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.Extensions
{
    public static class ServiceCollectionExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddFargoWriteDbContext(string? connectionString)
            {
                services.AddDbContext<FargoWriteDbContext>(opt =>
                    opt.UseSqlServer(
                        connectionString
                        ));

                return services;
            }

            public IServiceCollection AddFargoReadDbContext(string? connectionString)
            {
                services.AddDbContext<FargoReadDbContext>(opt =>
                    opt.UseSqlServer(
                        connectionString
                        ));

                return services;
            }

            public IServiceCollection AddFargoCurrentUser()
            {
                services.AddScoped<ICurrentUser, CurrentUser>();

                return services;
            }

            public IServiceCollection AddFargoInitializeSystemScope()
            {
                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

                return services;
            }

            public IServiceCollection AddFargoReadRepositoriesScopes()
            {
                services.AddScoped<IArticleReadRepository, ArticleReadRepository>();

                services.AddScoped<IItemReadRepository, ItemReadRepository>();

                services.AddScoped<IUserReadRepository, UserReadRepository>();

                return services;
            }

            public IServiceCollection AddFargoWriteRepositiresScopes()
            {
                services.AddScoped<IArticleRepository, ArticleRepository>();

                services.AddScoped<IItemRepository, ItemRepository>();

                services.AddScoped<IUserRepository, UserRepository>();

                services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

                return services;
            }

            public IServiceCollection AddFargoDomainServiceScopes()
            {
                services.AddScoped<ArticleService>();

                services.AddScoped<UserService>();

                return services;
            }

            public IServiceCollection AddFargoPasswordHasher()
            {
                services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

                return services;
            }

            public IServiceCollection AddFargoUnitOfWork()
            {
                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

                return services;
            }

            public IServiceCollection AddFargoScopes()
            {
                services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>();
                services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
                services.AddScoped<ICommandHandler<RefreshCommand, AuthResult>, RefreshCommandHandler>();

                services.AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>();

                services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleReadModel?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleReadModel>>, ArticleManyQueryHandler>();

                services.AddScoped<IQueryHandler<ItemSingleQuery, ItemReadModel?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemReadModel>>, ItemManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserSingleQuery, UserResponseModel?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, IReadOnlyCollection<UserResponseModel>>, UserManyQueryHandler>();

                services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

                services.AddScoped<ITokenHasher, Sha256TokenHasher>();

                services.AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();

                return services;
            }
        }
    }
}