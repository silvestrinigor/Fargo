using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.AuthModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.UserGroupModels;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Repositories;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Commands.UserGroupCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.UserGroupQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Application.Security;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Domain.Services;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Repositories;
using Fargo.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fargo.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for registering Fargo infrastructure services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        extension(IServiceCollection services)
        {
            /// <summary>
            /// Registers all Fargo infrastructure services required by the main application.
            /// </summary>
            /// <param name="services">
            /// The service collection used to register dependencies.
            /// </param>
            /// <param name="configuration">
            /// The application configuration used to bind options and connection strings.
            /// </param>
            /// <returns>
            /// The same <see cref="IServiceCollection"/> instance so that additional
            /// registrations can be chained.
            /// </returns>
            /// <remarks>
            /// This method registers:
            /// <list type="bullet">
            /// <item><description>JWT configuration options</description></item>
            /// <item><description>Database connection string options</description></item>
            /// <item><description>Write and read database contexts</description></item>
            /// <item><description>Repositories and query services</description></item>
            /// <item><description>Command and query handlers</description></item>
            /// <item><description>Domain services</description></item>
            /// <item><description>Security services</description></item>
            /// <item><description>Unit of work</description></item>
            /// <item><description>Current user abstraction</description></item>
            /// </list>
            /// </remarks>
            public IServiceCollection AddFargoInfrastructure(IConfiguration configuration)
            {
                AddFargoJwt(services, configuration);
                AddFargoConnectionStrings(services, configuration);
                AddDbContexts(services);
                AddRepositories(services);
                AddReadRepositories(services);
                AddHandlers(services);
                AddDomainServices(services);
                AddSecurity(services);
                AddPersistence(services);

                services.AddScoped<ICurrentUser, CurrentUser>();

                return services;
            }

            /// <summary>
            /// Registers only the infrastructure services required to execute database migrations.
            /// </summary>
            /// <param name="services">
            /// The service collection used to register dependencies.
            /// </param>
            /// <param name="configuration">
            /// The application configuration used to bind connection string options.
            /// </param>
            /// <returns>
            /// The same <see cref="IServiceCollection"/> instance so that additional
            /// registrations can be chained.
            /// </returns>
            public IServiceCollection AddFargoMigrationInfrastructure(IConfiguration configuration)
            {
                AddFargoConnectionStrings(services, configuration);
                AddDbContexts(services);

                return services;
            }

            /// <summary>
            /// Registers only the infrastructure services required to initialize or seed the system.
            /// </summary>
            /// <param name="services">
            /// The service collection used to register dependencies.
            /// </param>
            /// <param name="configuration">
            /// The application configuration used to bind connection string options.
            /// </param>
            /// <returns>
            /// The same <see cref="IServiceCollection"/> instance so that additional
            /// registrations can be chained.
            /// </returns>
            public IServiceCollection AddFargoSeedInfrastructure(IConfiguration configuration)
            {
                AddFargoConnectionStrings(services, configuration);
                AddDbContexts(services);
                AddRepositories(services);
                AddSecurity(services);
                AddPersistence(services);

                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

                services.AddScoped<ICurrentUser, SystemCurrentUser>();

                return services;
            }

            private IServiceCollection AddFargoJwt(
                    IConfiguration configuration
                    )
            {
                services
                    .AddOptions<JwtOptions>()
                    .Bind(configuration.GetSection(JwtOptions.SectionName))
                    .ValidateDataAnnotations()
                    .Validate(
                            o => o.Key.Length >= 32,
                            "Jwt:Key must be at least 32 characters long.")
                    .ValidateOnStart();

                return services;
            }

            private IServiceCollection AddFargoConnectionStrings(
                    IConfiguration configuration)
            {
                services
                    .AddOptions<ConnectionStringOptions>()
                    .Bind(configuration.GetSection(ConnectionStringOptions.SectionName))
                    .Validate(o => !string.IsNullOrWhiteSpace(o.Fargo),
                            "ConnectionStrings:Fargo must be provided.")
                    .ValidateOnStart();

                return services;
            }

            private void AddDbContexts()
            {
                services.AddDbContext<FargoWriteDbContext>((sp, opt) => ConfigureSqlServer(sp, opt));
                services.AddDbContext<FargoReadDbContext>((sp, opt) => ConfigureSqlServer(sp, opt));
            }

            private void AddRepositories()
            {
                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserGroupRepository, UserGroupRepository>();
                services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
                services.AddScoped<IPartitionRepository, PartitionRepository>();
            }

            private void AddReadRepositories()
            {
                services.AddScoped<IArticleQueries, ArticleQueries>();
                services.AddScoped<IItemQueries, ItemQueries>();
                services.AddScoped<IUserQueries, UserQueries>();
                services.AddScoped<IUserGroupQueries, UserGroupQueries>();
            }
            private void AddHandlers()
            {
                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

                services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>();
                services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
                services.AddScoped<ICommandHandler<RefreshCommand, AuthResult>, RefreshCommandHandler>();
                services.AddScoped<ICommandHandler<PasswordChangeCommand>, PasswordChangeCommandHandler>();

                services.AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>();

                services.AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>();
                services.AddScoped<ICommandHandler<UserAddUserGroupCommand>, UserAddUserGroupCommandHandler>();
                services.AddScoped<ICommandHandler<UserRemoveUserGroupCommand>, UserRemoveUserGroupCommandHandler>();

                services.AddScoped<ICommandHandler<UserGroupCreateCommand, Guid>, UserGroupCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserGroupDeleteCommand>, UserGroupDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserGroupUpdateCommand>, UserGroupUpdateCommandHandler>();

                services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleReadModel?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ArticleManyQuery, IReadOnlyCollection<ArticleReadModel>>, ArticleManyQueryHandler>();

                services.AddScoped<IQueryHandler<ItemSingleQuery, ItemReadModel?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemReadModel>>, ItemManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserSingleQuery, UserResponseModel?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, IReadOnlyCollection<UserResponseModel>>, UserManyQueryHandler>();
                services.AddScoped<IQueryHandler<UserUserGroupsManyQuery, IReadOnlyCollection<UserGroupResponseModel>>, UserUserGroupsManyQueryHandler>();

                services.AddScoped<IQueryHandler<UserGroupSingleQuery, UserGroupResponseModel?>, UserGroupSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupResponseModel>>, UserGroupManyQueryHandler>();
            }

            private void AddDomainServices()
            {
                services.AddScoped<ArticleService>();
                services.AddScoped<UserService>();
                services.AddScoped<UserGroupService>();
                services.AddScoped<PartitionService>();
            }

            private void AddSecurity()
            {
                services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
                services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
                services.AddScoped<ITokenHasher, Sha256TokenHasher>();
                services.AddScoped<IRefreshTokenGenerator, CryptoRefreshTokenGenerator>();
            }

            private void AddPersistence()
            {
                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();
            }

            private static void ConfigureSqlServer(IServiceProvider sp, DbContextOptionsBuilder opt)
            {
                var options = sp
                    .GetRequiredService<IOptions<ConnectionStringOptions>>()
                    .Value;

                opt.UseSqlServer(options.Fargo);
            }
        }
    }
}