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

            public IServiceCollection AddFargoScopes()
            {
                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

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
                services.AddScoped<IQueryHandler<ArticleManyQuery, IEnumerable<ArticleReadModel>>, ArticleManyQueryHandler>();

                services.AddScoped<IQueryHandler<ItemSingleQuery, ItemReadModel?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, IEnumerable<ItemReadModel>>, ItemManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserSingleQuery, UserResponseModel?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, IEnumerable<UserResponseModel>>, UserManyQueryHandler>();

                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IArticleReadRepository, ArticleReadRepository>();

                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IItemReadRepository, ItemReadRepository>();

                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserReadRepository, UserReadRepository>();

                services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

                services.AddScoped<ArticleService>();

                services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

                services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

                services.AddScoped<ITokenHasher, Sha256TokenHasher>();

                services.AddScoped<ICurrentUser, CurrentUser>();

                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

                return services;
            }
        }
    }
}